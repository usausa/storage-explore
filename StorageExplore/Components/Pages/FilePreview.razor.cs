using Microsoft.AspNetCore.Components;
using StorageExplore.Helpers;
using StorageExplore.Models;
using StorageExplore.Services;

namespace StorageExplore.Components.Pages;

public partial class FilePreview
{
    [Parameter, EditorRequired]
    public required FileItem Item { get; set; }

    [Parameter, EditorRequired]
    public required string Bucket { get; set; }

    [Parameter, EditorRequired]
    public required EventCallback OnClose { get; set; }

    [Inject]
    public FileStorageService Storage { get; set; } = default!;

    private string? textContent;

    private string PreviewUrl => $"/api/files/preview?bucket={Uri.EscapeDataString(Bucket)}&path={Uri.EscapeDataString(Item.RelativePath)}";
    private string DownloadUrl => $"/api/files/download?bucket={Uri.EscapeDataString(Bucket)}&path={Uri.EscapeDataString(Item.RelativePath)}";

    private bool IsImage => FileHelper.IsImage(Item.Extension);
    private bool IsVideo => FileHelper.IsVideo(Item.Extension);
    private bool IsAudio => FileHelper.IsAudio(Item.Extension);
    private bool IsPdf => FileHelper.IsPdf(Item.Extension);
    private bool IsText => FileHelper.IsText(Item.Extension);

    protected override async Task OnParametersSetAsync()
    {
        if (IsText)
        {
            textContent = await Storage.ReadTextAsync(Bucket, Item.RelativePath);
            textContent ??= "";
        }
    }
}
