namespace StorageExplore;

internal static partial class Log
{
    // Startup

    [LoggerMessage(Level = LogLevel.Information, Message = "Bucket directory created. name=[{Name}], path=[{Path}]")]
    public static partial void InfoBucketDirectoryCreated(this ILogger log, string name, string path);

    // Storage

    [LoggerMessage(Level = LogLevel.Warning, Message = "Path traversal attempt. bucket=[{Bucket}], path=[{Path}]")]
    public static partial void WarnPathTraversal(this ILogger log, string bucket, string path);

    [LoggerMessage(Level = LogLevel.Information, Message = "File saved. bucket=[{Bucket}], path=[{Path}], size=[{Size}]")]
    public static partial void InfoFileSaved(this ILogger log, string bucket, string path, long size);

    [LoggerMessage(Level = LogLevel.Information, Message = "Directory created. bucket=[{Bucket}], path=[{Path}]")]
    public static partial void InfoDirectoryCreated(this ILogger log, string bucket, string path);

    [LoggerMessage(Level = LogLevel.Information, Message = "File deleted. bucket=[{Bucket}], path=[{Path}]")]
    public static partial void InfoFileDeleted(this ILogger log, string bucket, string path);

    [LoggerMessage(Level = LogLevel.Information, Message = "Directory deleted. bucket=[{Bucket}], path=[{Path}]")]
    public static partial void InfoDirectoryDeleted(this ILogger log, string bucket, string path);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Invalid rename name. name=[{Name}]")]
    public static partial void WarnInvalidRenameName(this ILogger log, string name);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Rename path traversal attempt. bucket=[{Bucket}], path=[{Path}], name=[{Name}]")]
    public static partial void WarnRenamePathTraversal(this ILogger log, string bucket, string path, string name);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Rename target already exists. path=[{Path}]")]
    public static partial void WarnRenameTargetExists(this ILogger log, string path);

    [LoggerMessage(Level = LogLevel.Information, Message = "File renamed. bucket=[{Bucket}], path=[{Path}], name=[{Name}]")]
    public static partial void InfoFileRenamed(this ILogger log, string bucket, string path, string name);

    [LoggerMessage(Level = LogLevel.Information, Message = "Directory renamed. bucket=[{Bucket}], path=[{Path}], name=[{Name}]")]
    public static partial void InfoDirectoryRenamed(this ILogger log, string bucket, string path, string name);
}
