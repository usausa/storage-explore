namespace StorageExplore.Components.Pages;

using Microsoft.AspNetCore.Components;

using StorageExplore.Helpers;
using StorageExplore.Models;
using StorageExplore.Services;

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

    private string PreviewUrl => ApiRoutes.Preview(Bucket, Item.RelativePath);
    private string DownloadUrl => ApiRoutes.Download(Bucket, Item.RelativePath);

    //--------------------------------------------------------------------------------
    // Lifecycle
    //--------------------------------------------------------------------------------

    protected override async Task OnParametersSetAsync()
    {
        if (Item.IsText())
        {
            textContent = await Storage.ReadTextAsync(Bucket, Item.RelativePath);
            textContent ??= string.Empty;
        }
    }
}
