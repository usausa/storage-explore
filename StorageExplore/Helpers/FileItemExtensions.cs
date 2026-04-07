namespace StorageExplore.Helpers;

using StorageExplore.Models;

/// <summary>
/// Extension methods for <see cref="FileItem"/> that wrap <see cref="FileHelper"/> calls.
/// </summary>
public static class FileItemExtensions
{
    public static bool IsImage(this FileItem item) => FileHelper.IsImageExt(item.Extension);
    public static bool IsVideo(this FileItem item) => FileHelper.IsVideoExt(item.Extension);
    public static bool IsAudio(this FileItem item) => FileHelper.IsAudioExt(item.Extension);
    public static bool IsPdf(this FileItem item) => FileHelper.IsPdfExt(item.Extension);
    public static bool IsText(this FileItem item) => FileHelper.IsTextExt(item.Extension);
    public static bool IsPreviewable(this FileItem item) => !item.IsDirectory && FileHelper.IsPreviewable(item.Extension);
    public static bool HasThumbnail(this FileItem item) => FileHelper.HasThumbnail(item.Extension);
    public static string GetContentType(this FileItem item) => FileHelper.GetContentType(item.Extension);
    public static string GetIconCss(this FileItem item) => FileHelper.GetIconCss(item.Extension, item.IsDirectory);
    public static string FormatSize(this FileItem item) => item.IsDirectory ? string.Empty : FileHelper.FormatBytes(item.Size);
}
