namespace StorageExplore.Models;

public sealed class FileItem
{
    public required string Name { get; set; }
    public required string RelativePath { get; set; }
    public bool IsDirectory { get; set; }
    public long Size { get; set; }
    public DateTime LastModified { get; set; }
    public string Extension => IsDirectory ? "" : Path.GetExtension(Name).ToLowerInvariant();

    public string FormattedSize
    {
        get
        {
            if (IsDirectory) return "";
            return Size switch
            {
                < 1024 => $"{Size} B",
                < 1024 * 1024 => $"{Size / 1024.0:F1} KB",
                < 1024L * 1024 * 1024 => $"{Size / (1024.0 * 1024):F1} MB",
                _ => $"{Size / (1024.0 * 1024 * 1024):F2} GB"
            };
        }
    }

    public string IconCss => IsDirectory ? "bi-folder-fill text-warning" : Extension switch
    {
        ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" or ".svg" or ".ico" => "bi-file-image text-success",
        ".mp4" or ".avi" or ".mkv" or ".mov" or ".wmv" or ".webm" => "bi-file-play text-danger",
        ".mp3" or ".wav" or ".flac" or ".aac" or ".ogg" or ".wma" => "bi-file-music text-info",
        ".pdf" => "bi-file-pdf text-danger",
        ".doc" or ".docx" => "bi-file-word text-primary",
        ".xls" or ".xlsx" => "bi-file-excel text-success",
        ".ppt" or ".pptx" => "bi-file-ppt text-warning",
        ".zip" or ".rar" or ".7z" or ".tar" or ".gz" => "bi-file-zip text-secondary",
        ".txt" or ".log" or ".md" or ".csv" => "bi-file-text text-muted",
        ".cs" or ".js" or ".ts" or ".py" or ".java" or ".cpp" or ".h" or ".html" or ".css" or ".json" or ".xml" => "bi-file-code text-primary",
        _ => "bi-file-earmark text-muted"
    };

    public bool IsPreviewable => !IsDirectory && Extension is
        ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" or ".svg" or
        ".mp4" or ".webm" or
        ".mp3" or ".wav" or ".ogg" or
        ".txt" or ".log" or ".md" or ".csv" or ".json" or ".xml" or
        ".pdf";

    public string ContentType => Extension switch
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
