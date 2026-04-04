export function initDropZone(dropZoneElement, inputElement, dotNetHelper) {
    function preventDefaults(e) {
        e.preventDefault();
        e.stopPropagation();
    }

    function highlight() {
        dropZoneElement.classList.add('drop-zone-active');
    }

    function unhighlight() {
        dropZoneElement.classList.remove('drop-zone-active');
    }

    ['dragenter', 'dragover'].forEach(evt =>
        dropZoneElement.addEventListener(evt, e => { preventDefaults(e); highlight(); })
    );
    ['dragleave', 'drop'].forEach(evt =>
        dropZoneElement.addEventListener(evt, e => { preventDefaults(e); unhighlight(); })
    );

    dropZoneElement.addEventListener('drop', async e => {
        const items = e.dataTransfer.items;
        if (items) {
            const entries = [];
            for (const item of items) {
                const entry = item.webkitGetAsEntry?.();
                if (entry) entries.push(entry);
            }
            if (entries.length > 0) {
                const files = await collectFiles(entries);
                if (files.length > 0) {
                    await uploadFiles(files, dotNetHelper);
                }
                return;
            }
        }

        const files = e.dataTransfer.files;
        if (files.length > 0) {
            const fileList = [];
            for (const file of files) {
                fileList.push({ file, relativePath: file.name });
            }
            await uploadFiles(fileList, dotNetHelper);
        }
    });

    inputElement.addEventListener('change', async () => {
        const files = inputElement.files;
        if (files.length > 0) {
            const fileList = [];
            for (const file of files) {
                fileList.push({ file, relativePath: file.name });
            }
            await uploadFiles(fileList, dotNetHelper);
            inputElement.value = '';
        }
    });
}

async function collectFiles(entries) {
    const files = [];

    async function readEntry(entry, pathPrefix) {
        if (entry.isFile) {
            const file = await new Promise(resolve => entry.file(resolve));
            files.push({ file, relativePath: pathPrefix + file.name });
        } else if (entry.isDirectory) {
            const reader = entry.createReader();
            const subEntries = await new Promise(resolve => reader.readEntries(resolve));
            for (const subEntry of subEntries) {
                await readEntry(subEntry, pathPrefix + entry.name + '/');
            }
        }
    }

    for (const entry of entries) {
        await readEntry(entry, '');
    }
    return files;
}

async function uploadFiles(fileList, dotNetHelper) {
    const batchThreshold = 50 * 1024 * 1024; // 50MB per batch
    const totalFiles = fileList.length;
    const totalBytes = fileList.reduce((sum, f) => sum + f.file.size, 0);
    let completedFiles = 0;
    let completedBytes = 0;

    await dotNetHelper.invokeMethodAsync('OnUploadStarted', totalFiles, totalBytes);

    let batch = [];
    let batchSize = 0;

    for (const { file, relativePath } of fileList) {
        if (file.size > batchThreshold) {
            // Flush any pending small-file batch first
            if (batch.length > 0) {
                const batchBytes = batch.reduce((s, b) => s + b.file.size, 0);
                await sendBatch(batch, dotNetHelper);
                completedFiles += batch.length;
                completedBytes += batchBytes;
                await dotNetHelper.invokeMethodAsync('OnUploadByteProgress', completedFiles, totalFiles, completedBytes, totalBytes, '');
                batch = [];
                batchSize = 0;
            }
            // Upload large file with XHR progress
            await uploadLargeFile(file, relativePath, dotNetHelper, (loaded) => {
                dotNetHelper.invokeMethodAsync('OnUploadByteProgress',
                    completedFiles, totalFiles,
                    completedBytes + loaded, totalBytes,
                    file.name
                );
            });
            completedFiles++;
            completedBytes += file.size;
            await dotNetHelper.invokeMethodAsync('OnUploadByteProgress', completedFiles, totalFiles, completedBytes, totalBytes, '');
        } else {
            batch.push({ file, relativePath });
            batchSize += file.size;

            if (batchSize >= batchThreshold) {
                const batchBytes = batch.reduce((s, b) => s + b.file.size, 0);
                await sendBatch(batch, dotNetHelper);
                completedFiles += batch.length;
                completedBytes += batchBytes;
                await dotNetHelper.invokeMethodAsync('OnUploadByteProgress', completedFiles, totalFiles, completedBytes, totalBytes, '');
                batch = [];
                batchSize = 0;
            }
        }
    }

    if (batch.length > 0) {
        const batchBytes = batch.reduce((s, b) => s + b.file.size, 0);
        await sendBatch(batch, dotNetHelper);
        completedFiles += batch.length;
        completedBytes += batchBytes;
        await dotNetHelper.invokeMethodAsync('OnUploadByteProgress', completedFiles, totalFiles, completedBytes, totalBytes, '');
    }

    await dotNetHelper.invokeMethodAsync('OnUploadCompleted');
}

async function sendBatch(batch, dotNetHelper) {
    const currentPath = await dotNetHelper.invokeMethodAsync('GetCurrentPath');
    const currentBucket = await dotNetHelper.invokeMethodAsync('GetCurrentBucket');

    const byDir = new Map();
    for (const { file, relativePath } of batch) {
        const dirPart = relativePath.includes('/')
            ? relativePath.substring(0, relativePath.lastIndexOf('/'))
            : '';
        const uploadPath = dirPart
            ? (currentPath ? currentPath + '/' + dirPart : dirPart)
            : currentPath;

        if (!byDir.has(uploadPath)) byDir.set(uploadPath, []);
        byDir.get(uploadPath).push(file);
    }

    for (const [uploadPath, files] of byDir) {
        const fd = new FormData();
        for (const file of files) {
            fd.append('files', file, file.name);
        }
        const encodedBucket = encodeURIComponent(currentBucket);
        const encodedPath = uploadPath ? uploadPath.split('/').map(encodeURIComponent).join('/') : '';
        const response = await fetch(`/api/files/upload/${encodedBucket}/${encodedPath}`, {
            method: 'POST',
            body: fd
        });
        if (!response.ok) {
            const text = await response.text();
            console.error('Upload failed:', text);
            await dotNetHelper.invokeMethodAsync('OnUploadError', `Upload failed: ${response.statusText}`);
        }
    }
}

function uploadLargeFile(file, relativePath, dotNetHelper, onProgress) {
    return new Promise(async (resolve, reject) => {
        const currentPath = await dotNetHelper.invokeMethodAsync('GetCurrentPath');
        const currentBucket = await dotNetHelper.invokeMethodAsync('GetCurrentBucket');
        const dirPart = relativePath.includes('/')
            ? relativePath.substring(0, relativePath.lastIndexOf('/'))
            : '';
        const uploadPath = dirPart
            ? (currentPath ? currentPath + '/' + dirPart : dirPart)
            : currentPath;

        const fd = new FormData();
        fd.append('files', file, file.name);
        const encodedBucket = encodeURIComponent(currentBucket);
        const encodedPath = uploadPath ? uploadPath.split('/').map(encodeURIComponent).join('/') : '';

        const xhr = new XMLHttpRequest();
        xhr.open('POST', `/api/files/upload/${encodedBucket}/${encodedPath}`);

        xhr.upload.addEventListener('progress', (e) => {
            if (e.lengthComputable && onProgress) {
                onProgress(e.loaded);
            }
        });

        xhr.addEventListener('load', () => {
            if (xhr.status >= 200 && xhr.status < 300) {
                resolve();
            } else {
                console.error('Large file upload failed:', xhr.statusText);
                dotNetHelper.invokeMethodAsync('OnUploadError', `Upload failed for ${file.name}: ${xhr.statusText}`);
                resolve(); // Continue with other files
            }
        });

        xhr.addEventListener('error', () => {
            console.error('Large file upload network error');
            dotNetHelper.invokeMethodAsync('OnUploadError', `Network error uploading ${file.name}`);
            resolve();
        });

        xhr.send(fd);
    });
}
