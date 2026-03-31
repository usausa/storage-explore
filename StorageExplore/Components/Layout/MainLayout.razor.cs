using Microsoft.AspNetCore.Components;
using StorageExplore.Helpers;
using StorageExplore.Services;

namespace StorageExplore.Components.Layout;

public partial class MainLayout
{
    [Inject]
    public FileStorageService Storage { get; set; } = default!;

    [Inject]
    public NavigationManager Navigation { get; set; } = default!;

    private string CurrentBucket { get; set; } = "";
    private IReadOnlyList<string> BucketNames { get; set; } = [];
    private long TotalBytes { get; set; }
    private long FreeBytes { get; set; }

    private string TotalFormatted => FileHelper.FormatBytes(TotalBytes);
    private string UsedFormatted => FileHelper.FormatBytes(TotalBytes - FreeBytes);
    private int UsagePercent => TotalBytes > 0 ? (int)(100.0 * (TotalBytes - FreeBytes) / TotalBytes) : 0;

    private EventCallback<string> OnBucketChangedCallback => EventCallback.Factory.Create<string>(this, OnBucketChangedFromChild);

    protected override void OnInitialized()
    {
        BucketNames = Storage.Buckets.Keys.ToList();
        if (BucketNames.Count > 0)
        {
            CurrentBucket = BucketNames[0];
            UpdateStorageInfo();
        }
    }

    private void OnBucketChanged(ChangeEventArgs e)
    {
        var newBucket = e.Value?.ToString() ?? "";
        if (newBucket != CurrentBucket && Storage.Buckets.ContainsKey(newBucket))
        {
            CurrentBucket = newBucket;
            UpdateStorageInfo();
            Navigation.NavigateTo("/");
        }
    }

    private void OnBucketChangedFromChild(string newBucket)
    {
        if (newBucket != CurrentBucket && Storage.Buckets.ContainsKey(newBucket))
        {
            CurrentBucket = newBucket;
            UpdateStorageInfo();
            StateHasChanged();
        }
    }

    private void UpdateStorageInfo()
    {
        var (total, free) = Storage.GetStorageInfo(CurrentBucket);
        TotalBytes = total;
        FreeBytes = free;
    }
}
