namespace StorageExplore.Models;

using static StorageExplore.Helpers.FileHelper;

public sealed class FileItem
{
    public required string Name { get; set; }
    public required string RelativePath { get; set; }
    public bool IsDirectory { get; set; }
    public long Size { get; set; }
    public DateTime LastModified { get; set; }
    public string Extension => IsDirectory ? string.Empty : Path.GetExtension(Name).ToLowerInvariant();

    public string FormattedSize => IsDirectory ? string.Empty : FormatBytes(Size);
    public string IconCss => GetIconCss(Extension, IsDirectory);
    public bool IsPreviewable => !IsDirectory && IsPreviewable(Extension);
    public string ContentType => GetContentType(Extension);
}
