namespace StorageExplore.Services;

#pragma warning disable CA2227
public sealed class FileStorageSetting
{
    public const string SectionName = "Storage";

    /// <summary>
    /// Named root directories (buckets) that the file browser exposes.
    /// </summary>
    public required Dictionary<string, string> Buckets { get; set; }
}
#pragma warning restore CA2227
