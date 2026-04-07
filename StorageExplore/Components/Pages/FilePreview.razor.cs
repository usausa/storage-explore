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
    // Lifecycle
    //--------------------------------------------------------------------------------

    protected override async Task OnParametersSetAsync()
    {
        if (MediaHelper.IsTextFile(Item.Extension))
        {
            textContent = await Storage.ReadTextAsync(Bucket, Item.RelativePath);
            textContent ??= string.Empty;
        }
    }
}
