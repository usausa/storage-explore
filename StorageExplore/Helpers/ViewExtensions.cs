namespace StorageExplore.Helpers;

using StorageExplore.Models;

public static class ViewExtensions
{
    public static bool IsPreviewable(this FileItem item) => !item.IsDirectory && MediaHelper.IsPreviewable(item.Extension);

    public static string FormatSize(this FileItem item) => item.IsDirectory ? string.Empty : MediaHelper.FormatBytes(item.Size);
}
