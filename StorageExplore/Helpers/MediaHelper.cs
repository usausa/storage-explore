namespace StorageExplore.Helpers;

/// <summary>
/// Media type detection and content-type utilities.
/// </summary>
public static class MediaHelper
{
    // ---- Extension sets ----

    private static readonly HashSet<string> ImageExtensions =
        [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg", ".ico"];

    private static readonly HashSet<string> VideoExtensions =
        [".mp4", ".avi", ".mkv", ".mov", ".wmv", ".webm"];

    private static readonly HashSet<string> AudioExtensions =
        [".mp3", ".wav", ".flac", ".aac", ".ogg", ".wma"];

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

    public static bool IsImageFile(string extension) => ImageExtensions.Contains(extension);
    public static bool IsVideoFile(string extension) => VideoExtensions.Contains(extension);
    public static bool IsAudioFile(string extension) => AudioExtensions.Contains(extension);
    public static bool IsPdfFile(string extension) => extension == ".pdf";
    public static bool IsTextFile(string extension) => PreviewTextExtensions.Contains(extension);
    public static bool IsPreviewable(string extension) => PreviewableExtensions.Contains(extension);
    public static bool HasThumbnail(string extension) => ThumbnailExtensions.Contains(extension);

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
}
