namespace StorageExplore.Services;

using Microsoft.Extensions.Options;

using StorageExplore.Models;

// TODO check
#pragma warning disable CA3003
public sealed class FileStorageService
{
    private readonly StorageSettings settings;
    private readonly ILogger<FileStorageService> logger;

    public FileStorageService(IOptions<StorageSettings> options, ILogger<FileStorageService> logger)
    {
        settings = options.Value;
        this.logger = logger;

        foreach (var (name, path) in settings.Buckets)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                logger.LogInformation("Created bucket directory: {Name} -> {Path}", name, path);
            }
        }
    }

    public long MaxUploadSizeBytes => settings.MaxUploadSizeBytes;

    public IReadOnlyDictionary<string, string> Buckets => settings.Buckets;

    public string? GetBucketPath(string bucketName)
    {
        return settings.Buckets.GetValueOrDefault(bucketName);
    }

    /// <summary>
    /// Resolves a relative path within a bucket to a safe absolute path.
    /// Returns null if the bucket does not exist or the path escapes the root.
    /// </summary>
    public string? ResolvePath(string bucketName, string relativePath)
    {
        var bucketPath = GetBucketPath(bucketName);
        if (bucketPath is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return bucketPath;
        }

        var combined = Path.GetFullPath(Path.Combine(bucketPath, relativePath));
        if (!combined.StartsWith(bucketPath, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Path traversal attempt: {Bucket}/{Path}", bucketName, relativePath);
            return null;
        }
        return combined;
    }

    public List<FileItem> GetItems(string bucketName, string relativePath)
    {
        var fullPath = ResolvePath(bucketName, relativePath);
        if (fullPath is null || !Directory.Exists(fullPath))
        {
            return [];
        }

        var bucketPath = GetBucketPath(bucketName)!;
        var items = new List<FileItem>();

        foreach (var dir in Directory.EnumerateDirectories(fullPath))
        {
            var info = new DirectoryInfo(dir);
            items.Add(new FileItem
            {
                Name = info.Name,
                RelativePath = Path.GetRelativePath(bucketPath, dir).Replace('\\', '/'),
                IsDirectory = true,
                LastModified = info.LastWriteTime
            });
        }

        foreach (var file in Directory.EnumerateFiles(fullPath))
        {
            var info = new FileInfo(file);
            items.Add(new FileItem
            {
                Name = info.Name,
                RelativePath = Path.GetRelativePath(bucketPath, file).Replace('\\', '/'),
                IsDirectory = false,
                Size = info.Length,
                LastModified = info.LastWriteTime
            });
        }

        return items;
    }

    public FileItem? GetFileInfo(string bucketName, string relativePath)
    {
        var fullPath = ResolvePath(bucketName, relativePath);
        if (fullPath is null || !File.Exists(fullPath))
        {
            return null;
        }

        var info = new FileInfo(fullPath);
        return new FileItem
        {
            Name = info.Name,
            RelativePath = relativePath,
            IsDirectory = false,
            Size = info.Length,
            LastModified = info.LastWriteTime
        };
    }

    public Stream? OpenRead(string bucketName, string relativePath)
    {
        var fullPath = ResolvePath(bucketName, relativePath);
        if (fullPath is null || !File.Exists(fullPath))
        {
            return null;
        }

        return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    public async Task<string?> ReadTextAsync(string bucketName, string relativePath, int maxLength = 100_000)
    {
        var fullPath = ResolvePath(bucketName, relativePath);
        if (fullPath is null || !File.Exists(fullPath))
        {
            return null;
        }

        var info = new FileInfo(fullPath);
        if (info.Length > maxLength)
        {
            return null;
        }

        return await File.ReadAllTextAsync(fullPath);
    }

    public async Task SaveFileAsync(string bucketName, string relativePath, Stream content)
    {
        var fullPath = ResolvePath(bucketName, relativePath);
        if (fullPath is null)
        {
            throw new InvalidOperationException("Invalid path.");
        }

        var directory = Path.GetDirectoryName(fullPath)!;
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(fs);
        logger.LogInformation("Saved file: {Bucket}/{Path} ({Size} bytes)", bucketName, relativePath, fs.Length);
    }

    public void CreateDirectory(string bucketName, string relativePath)
    {
        var fullPath = ResolvePath(bucketName, relativePath);
        if (fullPath is null)
        {
            throw new InvalidOperationException("Invalid path.");
        }

        Directory.CreateDirectory(fullPath);
        logger.LogInformation("Created directory: {Bucket}/{Path}", bucketName, relativePath);
    }

    public void Delete(string bucketName, string relativePath)
    {
        var fullPath = ResolvePath(bucketName, relativePath);
        if (fullPath is null)
        {
            throw new InvalidOperationException("Invalid path.");
        }

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            logger.LogInformation("Deleted file: {Bucket}/{Path}", bucketName, relativePath);
        }
        else if (Directory.Exists(fullPath))
        {
            Directory.Delete(fullPath, recursive: true);
            logger.LogInformation("Deleted directory: {Bucket}/{Path}", bucketName, relativePath);
        }
    }

    /// <summary>
    /// Renames a file or directory. The newName must be a simple name (no path separators).
    /// Returns the new relative path on success, or null on failure.
    /// </summary>
    public string? Rename(string bucketName, string relativePath, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName) || newName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
        {
            logger.LogWarning("Invalid rename target name: {NewName}", newName);
            return null;
        }

        var fullPath = ResolvePath(bucketName, relativePath);
        if (fullPath is null)
        {
            return null;
        }

        var parentDir = System.IO.Path.GetDirectoryName(fullPath)!;
        var newFullPath = System.IO.Path.Combine(parentDir, newName);

        // Ensure the new path stays within the bucket
        var bucketPath = GetBucketPath(bucketName)!;
        if (!newFullPath.StartsWith(bucketPath, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Rename path traversal attempt: {Bucket}/{Path} -> {NewName}", bucketName, relativePath, newName);
            return null;
        }

        if (File.Exists(newFullPath) || Directory.Exists(newFullPath))
        {
            logger.LogWarning("Rename target already exists: {Path}", newFullPath);
            return null;
        }

        if (File.Exists(fullPath))
        {
            File.Move(fullPath, newFullPath);
            logger.LogInformation("Renamed file: {Bucket}/{OldPath} -> {NewName}", bucketName, relativePath, newName);
        }
        else if (Directory.Exists(fullPath))
        {
            Directory.Move(fullPath, newFullPath);
            logger.LogInformation("Renamed directory: {Bucket}/{OldPath} -> {NewName}", bucketName, relativePath, newName);
        }
        else
        {
            return null;
        }

        return System.IO.Path.GetRelativePath(bucketPath, newFullPath).Replace('\\', '/');
    }

    public bool Exists(string bucketName, string relativePath)
    {
        var fullPath = ResolvePath(bucketName, relativePath);
        if (fullPath is null)
        {
            return false;
        }
        return File.Exists(fullPath) || Directory.Exists(fullPath);
    }

    /// <summary>
    /// Gets the storage usage information for a bucket.
    /// </summary>
    public (long TotalBytes, long FreeBytes) GetStorageInfo(string bucketName)
    {
        var bucketPath = GetBucketPath(bucketName);
        if (bucketPath is null)
        {
            return (0, 0);
        }

        var driveInfo = new DriveInfo(Path.GetPathRoot(bucketPath)!);
        return (driveInfo.TotalSize, driveInfo.AvailableFreeSpace);
    }
}
