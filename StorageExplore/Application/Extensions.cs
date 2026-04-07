namespace StorageExplore.Application;

using StorageExplore.Helpers;
using StorageExplore.Models;

public static class Extensions
{
    public static bool IsPreviewable(this FileItem item) => !item.IsDirectory && MediaHelper.IsPreviewable(item.Extension);
}
