namespace StorageExplore.Components.Layout;

using Microsoft.AspNetCore.Components;

using StorageExplore.Services;

using static StorageExplore.Application.ViewHelper;

public partial class MainLayout
{
    //--------------------------------------------------------------------------------
    // Parameter
    //--------------------------------------------------------------------------------

    [Inject]
    public FileStorageService Storage { get; set; } = default!;

    [Inject]
    public NavigationManager Navigation { get; set; } = default!;

    //--------------------------------------------------------------------------------
    // State
    //--------------------------------------------------------------------------------

    private string CurrentBucket { get; set; } = string.Empty;
    private List<string> BucketNames { get; set; } = [];
    private long TotalBytes { get; set; }
    private long FreeBytes { get; set; }

    //--------------------------------------------------------------------------------
    // Data
    //--------------------------------------------------------------------------------

    private string TotalFormatted => FormatBytes(TotalBytes);
    private string UsedFormatted => FormatBytes(TotalBytes - FreeBytes);
    private int UsagePercent => TotalBytes > 0 ? (int)(100.0 * (TotalBytes - FreeBytes) / TotalBytes) : 0;

    private EventCallback<string> OnBucketChangedCallback => EventCallback.Factory.Create<string>(this, OnBucketChangedFromChild);

    //--------------------------------------------------------------------------------
    // Lifecycle
    //--------------------------------------------------------------------------------

    protected override void OnInitialized()
    {
        BucketNames = Storage.Buckets.Keys.ToList();
        if (BucketNames.Count > 0)
        {
            CurrentBucket = BucketNames[0];
            UpdateStorageInfo();
        }
    }

    //--------------------------------------------------------------------------------
    // Action
    //--------------------------------------------------------------------------------

    private void OnBucketChanged(ChangeEventArgs e)
    {
        var newBucket = e.Value?.ToString() ?? string.Empty;
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

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private void UpdateStorageInfo()
    {
        var (total, free) = Storage.GetStorageInfo(CurrentBucket);
        TotalBytes = total;
        FreeBytes = free;
    }
}
