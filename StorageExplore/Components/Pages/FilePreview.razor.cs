namespace StorageExplore.Components.Pages;

using Microsoft.AspNetCore.Components;

using StorageExplore.Models;
using StorageExplore.Services;

using static StorageExplore.Helpers.FileHelper;

public partial class FilePreview
{
    //--------------------------------------------------------------------------------
    // Parameter
    //--------------------------------------------------------------------------------

    [Parameter]
    [EditorRequired]
    public required FileItem Item { get; set; }

    [Parameter]
    [EditorRequired]
    public required string Bucket { get; set; }

    [Parameter]
    [EditorRequired]
    public required EventCallback OnClose { get; set; }

    [Inject]
    public FileStorageService Storage { get; set; } = default!;

    //--------------------------------------------------------------------------------
    // State
    //--------------------------------------------------------------------------------

    private string? textContent;

    //--------------------------------------------------------------------------------
    // Data
    //--------------------------------------------------------------------------------

    private string PreviewUrl => $"/api/files/preview/{Uri.EscapeDataString(Bucket)}/{EncodePathSegments(Item.RelativePath)}";
    private string DownloadUrl => $"/api/files/download/{Uri.EscapeDataString(Bucket)}/{EncodePathSegments(Item.RelativePath)}";

    private bool IsImage => IsImageExt(Item.Extension);
    private bool IsVideo => IsVideoExt(Item.Extension);
    private bool IsAudio => IsAudioExt(Item.Extension);
    private bool IsPdf => IsPdfExt(Item.Extension);
    private bool IsText => IsTextExt(Item.Extension);

    //--------------------------------------------------------------------------------
    // Lifecycle
    //--------------------------------------------------------------------------------

    protected override async Task OnParametersSetAsync()
    {
        if (IsText)
        {
            textContent = await Storage.ReadTextAsync(Bucket, Item.RelativePath);
            textContent ??= string.Empty;
        }
    }
}
