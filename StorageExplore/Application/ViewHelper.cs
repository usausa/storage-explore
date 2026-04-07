namespace StorageExplore.Application;

using Microsoft.AspNetCore.Components;

using StorageExplore.Helpers;
using StorageExplore.Models;

public static class ViewHelper
{
    //--------------------------------------------------------------------------------
    // Format
    //--------------------------------------------------------------------------------

    public static string FormatBytes(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
        < 1024L * 1024 * 1024 => $"{bytes / (1024.0 * 1024):F1} MB",
        < 1024L * 1024 * 1024 * 1024 => $"{bytes / (1024.0 * 1024 * 1024):F1} GB",
        _ => $"{bytes / (1024.0 * 1024 * 1024 * 1024):F2} TB"
    };

    public static string FormatSize(FileItem item) =>
        item.IsDirectory ? string.Empty : FormatBytes(item.Size);

    //--------------------------------------------------------------------------------
    // Sort
    //--------------------------------------------------------------------------------

    public static MarkupString SortIndicator(bool isActive, bool descending) =>
        isActive
            ? new MarkupString(descending
                ? "<i class=\"bi bi-chevron-down\" style=\"font-size:0.7rem\"></i>"
                : "<i class=\"bi bi-chevron-up\" style=\"font-size:0.7rem\"></i>")
            : new MarkupString(string.Empty);

    //--------------------------------------------------------------------------------
    // Icon css
    //--------------------------------------------------------------------------------

    private static readonly HashSet<string> ArchiveExtensions =
        [".zip", ".rar", ".7z", ".tar", ".gz"];

    private static readonly HashSet<string> TextExtensions =
        [".txt", ".log", ".md", ".csv"];

    private static readonly HashSet<string> CodeExtensions =
        [".cs", ".js", ".ts", ".py", ".java", ".cpp", ".h", ".html", ".css", ".json", ".xml"];

    public static string GetIconCss(FileItem item)
    {
        var ext = item.Extension;

        if (item.IsDirectory)
        {
            return "bi-folder-fill text-warning";
        }
        if (MediaHelper.IsImageFile(ext))
        {
            return "bi-file-image text-success";
        }
        if (MediaHelper.IsVideoFile(ext))
        {
            return "bi-file-play text-danger";
        }
        if (MediaHelper.IsAudioFile(ext))
        {
            return "bi-file-music text-info";
        }
        if (ext == ".pdf")
        {
            return "bi-file-pdf text-danger";
        }
        if (ext is ".doc" or ".docx")
        {
            return "bi-file-word text-primary";
        }
        if (ext is ".xls" or ".xlsx")
        {
            return "bi-file-excel text-success";
        }
        if (ext is ".ppt" or ".pptx")
        {
            return "bi-file-ppt text-warning";
        }
        if (ArchiveExtensions.Contains(ext))
        {
            return "bi-file-zip text-secondary";
        }
        if (TextExtensions.Contains(ext))
        {
            return "bi-file-text text-muted";
        }
        if (CodeExtensions.Contains(ext))
        {
            return "bi-file-code text-primary";
        }

        return "bi-file-earmark text-muted";
    }
}
