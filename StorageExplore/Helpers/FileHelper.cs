namespace StorageExplore.Helpers;

/// <summary>
/// Shared utility methods for file type detection, formatting, and icon mapping.
/// </summary>
public static class FileHelper
{
    // ---- Extension sets ----

    private static readonly HashSet<string> ImageExtensions =
        [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg", ".ico"];

    private static readonly HashSet<string> VideoExtensions =
        [".mp4", ".avi", ".mkv", ".mov", ".wmv", ".webm"];

    private static readonly HashSet<string> AudioExtensions =
        [".mp3", ".wav", ".flac", ".aac", ".ogg", ".wma"];

    private static readonly HashSet<string> ArchiveExtensions =
        [".zip", ".rar", ".7z", ".tar", ".gz"];

    private static readonly HashSet<string> TextExtensions =
        [".txt", ".log", ".md", ".csv"];

    private static readonly HashSet<string> CodeExtensions =
        [".cs", ".js", ".ts", ".py", ".java", ".cpp", ".h", ".html", ".css", ".json", ".xml"];

    private static readonly HashSet<string> PreviewableExtensions =
        [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg",
         ".mp4", ".webm",
         ".mp3", ".wav", ".ogg",
         ".txt", ".log", ".md", ".csv", ".json", ".xml",
         ".pdf"];

    private static readonly HashSet<string> ThumbnailExtensions =
        [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg", ".ico"];

    private static readonly HashSet<string> PreviewTextExtensions =
        [".txt", ".log", ".md", ".csv", ".json", ".xml"];

    // ---- Type checks ----

    public static bool IsImage(string extension) => ImageExtensions.Contains(extension);
    public static bool IsVideo(string extension) => VideoExtensions.Contains(extension);
    public static bool IsAudio(string extension) => AudioExtensions.Contains(extension);
    public static bool IsPdf(string extension) => extension == ".pdf";
    public static bool IsText(string extension) => PreviewTextExtensions.Contains(extension);
    public static bool IsPreviewable(string extension) => PreviewableExtensions.Contains(extension);
    public static bool HasThumbnail(string extension) => ThumbnailExtensions.Contains(extension);

    // ---- Size formatting ----

    public static string FormatBytes(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
        < 1024L * 1024 * 1024 => $"{bytes / (1024.0 * 1024):F1} MB",
        < 1024L * 1024 * 1024 * 1024 => $"{bytes / (1024.0 * 1024 * 1024):F1} GB",
        _ => $"{bytes / (1024.0 * 1024 * 1024 * 1024):F2} TB"
    };

    // ---- Icon CSS class ----

    public static string GetIconCss(string extension, bool isDirectory)
    {
        if (isDirectory)
        {
            return "bi-folder-fill text-warning";
        }

        if (ImageExtensions.Contains(extension))
        {
            return "bi-file-image text-success";
        }
        if (VideoExtensions.Contains(extension))
        {
            return "bi-file-play text-danger";
        }
        if (AudioExtensions.Contains(extension))
        {
            return "bi-file-music text-info";
        }
        if (extension == ".pdf")
        {
            return "bi-file-pdf text-danger";
        }
        if (extension is ".doc" or ".docx")
        {
            return "bi-file-word text-primary";
        }
        if (extension is ".xls" or ".xlsx")
        {
            return "bi-file-excel text-success";
        }
        if (extension is ".ppt" or ".pptx")
        {
            return "bi-file-ppt text-warning";
        }
        if (ArchiveExtensions.Contains(extension))
        {
            return "bi-file-zip text-secondary";
        }
        if (TextExtensions.Contains(extension))
        {
            return "bi-file-text text-muted";
        }
        if (CodeExtensions.Contains(extension))
        {
            return "bi-file-code text-primary";
        }

        return "bi-file-earmark text-muted";
    }

    // ---- Content-Type mapping ----

    public static string GetContentType(string extension) => extension switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".gif" => "image/gif",
        ".bmp" => "image/bmp",
        ".webp" => "image/webp",
        ".svg" => "image/svg+xml",
        ".ico" => "image/x-icon",
        ".mp4" => "video/mp4",
        ".webm" => "video/webm",
        ".mp3" => "audio/mpeg",
        ".wav" => "audio/wav",
        ".ogg" => "audio/ogg",
        ".pdf" => "application/pdf",
        ".txt" or ".log" or ".csv" => "text/plain",
        ".md" => "text/markdown",
        ".json" => "application/json",
        ".xml" => "application/xml",
        ".html" => "text/html",
        ".css" => "text/css",
        ".js" => "application/javascript",
        _ => "application/octet-stream"
    };

    // ---- URL path encoding ----

    /// <summary>
    /// Encodes each segment of a relative path for use in a URL path.
    /// Slashes are preserved; individual segments are percent-encoded.
    /// </summary>
    public static string EncodePathSegments(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        return string.Join('/', path.Split('/').Select(Uri.EscapeDataString));
    }
}
