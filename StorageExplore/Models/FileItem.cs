namespace StorageExplore.Models;

public sealed class FileItem
{
    public required string Name { get; set; }

    public required string RelativePath { get; set; }

    public bool IsDirectory { get; set; }

    public long Size { get; set; }

    public DateTime LastModified { get; set; }

#pragma warning disable CA1308
    public string Extension => IsDirectory ? string.Empty : Path.GetExtension(Name).ToLowerInvariant();
#pragma warning restore CA1308
}
