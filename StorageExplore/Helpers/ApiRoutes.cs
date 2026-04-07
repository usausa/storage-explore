namespace StorageExplore.Helpers;

/// <summary>
/// Builds API endpoint URLs for file operations.
/// </summary>
public static class ApiRoutes
{
    public static string Download(string bucket, string path) =>
        $"/api/files/download/{Uri.EscapeDataString(bucket)}/{EncodePathSegments(path)}";

    public static string Preview(string bucket, string path) =>
        $"/api/files/preview/{Uri.EscapeDataString(bucket)}/{EncodePathSegments(path)}";

    public static string Thumbnail(string bucket, string path, long ticks) =>
        $"/api/files/thumbnail/{Uri.EscapeDataString(bucket)}/{EncodePathSegments(path)}?t={ticks}";

    private static string EncodePathSegments(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        return string.Join('/', path.Split('/').Select(Uri.EscapeDataString));
    }
}
