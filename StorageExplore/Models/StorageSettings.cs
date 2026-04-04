namespace StorageExplore.Models;

#pragma warning disable CA2227
public sealed class StorageSettings
{
    public const string SectionName = "Storage";

    /// <summary>
    /// Named root directories (buckets) that the file browser exposes.
    /// </summary>
    public required Dictionary<string, string> Buckets { get; set; }

    /// <summary>
    /// Maximum upload file size in bytes. Default is 10 GB.
    /// </summary>
    public long MaxUploadSizeBytes { get; set; } = 10L * 1024 * 1024 * 1024;
}
#pragma warning restore CA2227
