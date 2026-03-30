using Microsoft.AspNetCore.Components;
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

    private bool IsImage => Item.Extension is ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" or ".svg";
    private bool IsVideo => Item.Extension is ".mp4" or ".webm";
    private bool IsAudio => Item.Extension is ".mp3" or ".wav" or ".ogg";
    private bool IsPdf => Item.Extension is ".pdf";
    private bool IsText => Item.Extension is ".txt" or ".log" or ".md" or ".csv" or ".json" or ".xml";

    protected override async Task OnParametersSetAsync()
    {
        if (IsText)
        {
            textContent = await Storage.ReadTextAsync(Bucket, Item.RelativePath);
            textContent ??= "";
        }
    }
}
