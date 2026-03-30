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
    const chunkSize = 50 * 1024 * 1024; // 50MB per request
    const totalFiles = fileList.length;
    let completedFiles = 0;

    await dotNetHelper.invokeMethodAsync('OnUploadStarted', totalFiles);

    let batch = [];
    let batchSize = 0;

    for (const { file, relativePath } of fileList) {
        if (file.size > chunkSize) {
            if (batch.length > 0) {
                await sendBatch(batch, dotNetHelper);
                completedFiles += batch.length;
                await dotNetHelper.invokeMethodAsync('OnUploadProgress', completedFiles, totalFiles);
                batch = [];
                batchSize = 0;
            }
            await uploadLargeFile(file, relativePath, dotNetHelper);
            completedFiles++;
            await dotNetHelper.invokeMethodAsync('OnUploadProgress', completedFiles, totalFiles);
        } else {
            batch.push({ file, relativePath });
            batchSize += file.size;

            if (batchSize >= chunkSize) {
                await sendBatch(batch, dotNetHelper);
                completedFiles += batch.length;
                await dotNetHelper.invokeMethodAsync('OnUploadProgress', completedFiles, totalFiles);
                batch = [];
                batchSize = 0;
            }
        }
    }

    if (batch.length > 0) {
        await sendBatch(batch, dotNetHelper);
        completedFiles += batch.length;
        await dotNetHelper.invokeMethodAsync('OnUploadProgress', completedFiles, totalFiles);
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
        const encodedPath = encodeURIComponent(uploadPath);
        const response = await fetch(`/api/files/upload?bucket=${encodedBucket}&path=${encodedPath}`, {
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

async function uploadLargeFile(file, relativePath, dotNetHelper) {
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
    const encodedPath = encodeURIComponent(uploadPath);
    const response = await fetch(`/api/files/upload?bucket=${encodedBucket}&path=${encodedPath}`, {
        method: 'POST',
        body: fd
    });
    if (!response.ok) {
        const text = await response.text();
        console.error('Large file upload failed:', text);
        await dotNetHelper.invokeMethodAsync('OnUploadError', `Upload failed for ${file.name}: ${response.statusText}`);
    }
}
