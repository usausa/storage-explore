namespace StorageExplore.Services;

// ReSharper disable CollectionNeverUpdated.Global
#pragma warning disable CA2227
public sealed class FileStorageSetting
{
    public required Dictionary<string, string> Buckets { get; set; }
}
#pragma warning restore CA2227
// ReSharper restore CollectionNeverUpdated.Global
