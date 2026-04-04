using StorageExplore.Helpers;

namespace StorageExplore.Models;

public sealed class FileItem
{
    public required string Name { get; set; }
    public required string RelativePath { get; set; }
    public bool IsDirectory { get; set; }
    public long Size { get; set; }
    public DateTime LastModified { get; set; }
    public string Extension => IsDirectory ? string.Empty : Path.GetExtension(Name).ToLowerInvariant();

    public string FormattedSize => IsDirectory ? string.Empty : FileHelper.FormatBytes(Size);
    public string IconCss => FileHelper.GetIconCss(Extension, IsDirectory);
    public bool IsPreviewable => !IsDirectory && FileHelper.IsPreviewable(Extension);
    public string ContentType => FileHelper.GetContentType(Extension);
}
