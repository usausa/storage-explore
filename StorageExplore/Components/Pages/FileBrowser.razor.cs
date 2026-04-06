namespace StorageExplore.Components.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

using StorageExplore.Helpers;
using StorageExplore.Models;
using StorageExplore.Services;

public partial class FileBrowser : IAsyncDisposable
{
    [Parameter]
    public string? Path { get; set; }

    [CascadingParameter(Name = "Bucket")]
    public string Bucket { get; set; } = string.Empty;

    [Inject]
    public FileStorageService Storage { get; set; } = default!;

    [Inject]
    public NavigationManager Navigation { get; set; } = default!;

    [Inject]
    public IJSRuntime JS { get; set; } = default!;

    private List<FileItem> items = [];
    private FileItem? selectedItem;
    private FileItem? previewItem;
    private bool isLoading;
    private bool isUploading;
    private int uploadedCount;
    private int uploadTotalCount;
    private long uploadedBytes;
    private long uploadTotalBytes;
    private string uploadCurrentFile = string.Empty;
    private string? uploadError;
    private bool showNewFolder;
    private string newFolderName = string.Empty;
    private bool showDeleteConfirm;
    private ViewMode viewMode = ViewMode.List;
    private SortField sortField = SortField.Name;
    private bool sortDescending;

    // Rename state
    private FileItem? renamingItem;
    private string renameValue = string.Empty;
    private string? renameError;

    // Context menu state
    private bool showContextMenu;
    private double contextMenuX;
    private double contextMenuY;
    private FileItem? contextMenuItem;

    // Overwrite confirmation state
    private bool showOverwriteConfirm;
    private List<string> overwriteFileNames = [];
    private TaskCompletionSource<bool>? overwriteTcs;

    private ElementReference dropZoneRef;
    private ElementReference fileInputRef;
    private IJSObjectReference? jsModule;
    private DotNetObjectReference<FileBrowser>? dotNetRef;

    private string previousBucket = string.Empty;
    private bool isInitialized;

    protected override Task OnParametersSetAsync()
    {
        if (!isInitialized)
        {
            previousBucket = Bucket;
            isInitialized = true;
        }
        else if (previousBucket != Bucket)
        {
            previousBucket = Bucket;
            Path = null;
        }

        return LoadItems();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            dotNetRef = DotNetObjectReference.Create(this);
            jsModule = await JS.InvokeAsync<IJSObjectReference>("import", "./js/fileUpload.js");
            await jsModule.InvokeVoidAsync("initDropZone", dropZoneRef, fileInputRef, dotNetRef);
        }
    }

    private async Task LoadItems()
    {
        isLoading = true;
        selectedItem = null;
        try
        {
            var bucket = Bucket;
            var path = Path ?? string.Empty;
            items = await Task.Run(() => Storage.GetItems(bucket, path));
        }
        finally
        {
            isLoading = false;
        }
    }

    private void NavigateTo(string path)
    {
        selectedItem = null;
        previewItem = null;
        Navigation.NavigateTo(string.IsNullOrEmpty(path) ? "/" : $"/browse/{path}");
    }

    private void NavigateUp()
    {
        if (string.IsNullOrEmpty(Path))
        {
            return;
        }

        var lastSlash = Path.LastIndexOf('/');
        var parentPath = lastSlash > 0 ? Path[..lastSlash] : string.Empty;
        NavigateTo(parentPath);
    }

    private void SelectItem(FileItem item)
    {
        if (item.IsPreviewable)
        {
            selectedItem = item;
            previewItem = item;
        }
        else
        {
            selectedItem = selectedItem == item ? null : item;
        }
    }

    private void OpenItem(FileItem item)
    {
        if (item.IsPreviewable)
        {
            previewItem = item;
        }
        else
        {
            Navigation.NavigateTo(GetDownloadUrl(item), forceLoad: true);
        }
    }

    private void DeleteItem(FileItem item)
    {
        selectedItem = item;
        showDeleteConfirm = true;
    }

    private void ClosePreview()
    {
        previewItem = null;
    }

    private void ShowUploadDialog()
    {
        JS.InvokeVoidAsync("eval", "document.querySelector('.fb input[type=file]').click()");
    }

    private void ShowNewFolderDialog()
    {
        newFolderName = string.Empty;
        showNewFolder = true;
    }

    private Task CreateFolder()
    {
        if (string.IsNullOrWhiteSpace(newFolderName))
        {
            return Task.CompletedTask;
        }

        var folderPath = string.IsNullOrEmpty(Path) ? newFolderName : $"{Path}/{newFolderName}";
        Storage.CreateDirectory(Bucket, folderPath);
        showNewFolder = false;
        newFolderName = string.Empty;
        return LoadItems();
    }

    private Task OnNewFolderKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            return CreateFolder();
        }
        else if (e.Key == "Escape")
        {
            showNewFolder = false;
        }
        return Task.CompletedTask;
    }

    private void DeleteSelected()
    {
        if (selectedItem is null)
        {
            return;
        }
        showDeleteConfirm = true;
    }

    private Task ConfirmDelete()
    {
        if (selectedItem is null)
        {
            return Task.CompletedTask;
        }

        Storage.Delete(Bucket, selectedItem.RelativePath);
        showDeleteConfirm = false;
        selectedItem = null;
        return LoadItems();
    }

    // ---- Rename ----

    private void StartRename(FileItem item)
    {
        renamingItem = item;
        renameValue = item.Name;
        renameError = null;
    }

    private Task ConfirmRename()
    {
        if (renamingItem is null || string.IsNullOrWhiteSpace(renameValue))
        {
            return Task.CompletedTask;
        }

        if (renameValue == renamingItem.Name)
        {
            CancelRename();
            return Task.CompletedTask;
        }

        var newPath = Storage.Rename(Bucket, renamingItem.RelativePath, renameValue.Trim());
        if (newPath is null)
        {
            renameError = "Rename failed. Name may already exist or contain invalid characters.";
            return Task.CompletedTask;
        }

        renamingItem = null;
        renameValue = string.Empty;
        renameError = null;
        return LoadItems();
    }

    private void CancelRename()
    {
        renamingItem = null;
        renameValue = string.Empty;
        renameError = null;
    }

    private Task OnRenameKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            return ConfirmRename();
        }
        else if (e.Key == "Escape")
        {
            CancelRename();
        }
        return Task.CompletedTask;
    }

    // ---- Context menu ----

    private void OnContextMenu(MouseEventArgs e, FileItem item)
    {
        contextMenuItem = item;
        selectedItem = item;
        contextMenuX = e.ClientX;
        contextMenuY = e.ClientY;
        showContextMenu = true;
    }

    private void CloseContextMenu()
    {
        showContextMenu = false;
        contextMenuItem = null;
    }

    private void ContextMenuOpen()
    {
        if (contextMenuItem is null)
        {
            return;
        }
        var item = contextMenuItem;
        CloseContextMenu();

        if (item.IsDirectory)
        {
            NavigateTo(item.RelativePath);
        }
        else
        {
            OpenItem(item);
        }
    }

    private void ContextMenuRename()
    {
        if (contextMenuItem is null)
        {
            return;
        }
        var item = contextMenuItem;
        CloseContextMenu();
        StartRename(item);
    }

    private void ContextMenuDelete()
    {
        if (contextMenuItem is null)
        {
            return;
        }
        selectedItem = contextMenuItem;
        CloseContextMenu();
        showDeleteConfirm = true;
    }

    private string GetDownloadUrl(FileItem item)
    {
        return $"/api/files/download/{Uri.EscapeDataString(Bucket)}/{FileHelper.EncodePathSegments(item.RelativePath)}";
    }

    private string GetThumbnailUrl(FileItem item)
    {
        return $"/api/files/thumbnail/{Uri.EscapeDataString(Bucket)}/{FileHelper.EncodePathSegments(item.RelativePath)}?t={item.LastModified.Ticks}";
    }

    private static bool IsImageFile(FileItem item) => FileHelper.HasThumbnail(item.Extension);

    private List<Breadcrumb> GetBreadcrumbs()
    {
        if (string.IsNullOrEmpty(Path))
        {
            return [];
        }

        var parts = Path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var crumbs = new List<Breadcrumb>();
        var accumulated = string.Empty;

        for (var i = 0; i < parts.Length; i++)
        {
            accumulated = i == 0 ? parts[i] : $"{accumulated}/{parts[i]}";
            crumbs.Add(new Breadcrumb
            {
                Name = parts[i],
                Path = accumulated,
                Url = $"/browse/{accumulated}",
                IsLast = i == parts.Length - 1
            });
        }

        return crumbs;
    }

    private void SortBy(SortField field)
    {
        if (sortField == field)
        {
            sortDescending = !sortDescending;
        }
        else
        {
            sortField = field;
            sortDescending = false;
        }
    }

    private IEnumerable<FileItem> GetSortedItems()
    {
        var dirs = items.Where(i => i.IsDirectory);
        var files = items.Where(i => !i.IsDirectory);

        dirs = sortField switch
        {
            SortField.Name => sortDescending ? dirs.OrderByDescending(i => i.Name) : dirs.OrderBy(i => i.Name),
            SortField.Modified => sortDescending ? dirs.OrderByDescending(i => i.LastModified) : dirs.OrderBy(i => i.LastModified),
            _ => dirs.OrderBy(i => i.Name)
        };

        files = sortField switch
        {
            SortField.Name => sortDescending ? files.OrderByDescending(i => i.Name) : files.OrderBy(i => i.Name),
            SortField.Size => sortDescending ? files.OrderByDescending(i => i.Size) : files.OrderBy(i => i.Size),
            SortField.Modified => sortDescending ? files.OrderByDescending(i => i.LastModified) : files.OrderBy(i => i.LastModified),
            _ => files.OrderBy(i => i.Name)
        };

        return dirs.Concat(files);
    }

    private MarkupString SortIndicator(SortField field)
    {
        if (sortField != field)
        {
            return new MarkupString(string.Empty);
        }
        return new MarkupString(sortDescending
            ? "<i class=\"bi bi-chevron-down\" style=\"font-size:0.7rem\"></i>"
            : "<i class=\"bi bi-chevron-up\" style=\"font-size:0.7rem\"></i>");
    }

    [JSInvokable]
    public string GetCurrentPath() => Path ?? string.Empty;

    [JSInvokable]
    public string GetCurrentBucket() => Bucket;

    [JSInvokable]
    public async Task<bool> CheckDuplicates(string[] fileNames)
    {
        var existingNames = items
            .Where(i => !i.IsDirectory)
            .Select(i => i.Name)
            .ToHashSet(StringComparer.Ordinal);

        var duplicates = fileNames.Where(existingNames.Contains).ToList();
        if (duplicates.Count == 0)
        {
            return true;
        }

        overwriteFileNames = duplicates;
        showOverwriteConfirm = true;
        overwriteTcs = new TaskCompletionSource<bool>();
        StateHasChanged();

        return await overwriteTcs.Task;
    }

    private void ConfirmOverwrite()
    {
        showOverwriteConfirm = false;
        overwriteTcs?.TrySetResult(true);
    }

    private void CancelOverwrite()
    {
        showOverwriteConfirm = false;
        overwriteTcs?.TrySetResult(false);
    }

    [JSInvokable]
    public void OnUploadStarted(int totalCount, long totalBytes)
    {
        isUploading = true;
        uploadedCount = 0;
        uploadTotalCount = totalCount;
        uploadedBytes = 0;
        uploadTotalBytes = totalBytes;
        uploadCurrentFile = string.Empty;
        uploadError = null;
        InvokeAsync(StateHasChanged);
    }

    [JSInvokable]
    public void OnUploadByteProgress(int completedFiles, int totalFiles, long completedBytes, long totalBytes, string currentFile)
    {
        uploadedCount = completedFiles;
        uploadTotalCount = totalFiles;
        uploadedBytes = completedBytes;
        uploadTotalBytes = totalBytes;
        uploadCurrentFile = currentFile;
        InvokeAsync(StateHasChanged);
    }

    [JSInvokable]
    public async Task OnUploadCompleted()
    {
        isUploading = false;
        uploadCurrentFile = string.Empty;
        await LoadItems();
        await InvokeAsync(StateHasChanged);
    }

    [JSInvokable]
    public void OnUploadError(string error)
    {
        uploadError = error;
        InvokeAsync(StateHasChanged);
    }

    public ValueTask DisposeAsync()
    {
        dotNetRef?.Dispose();
        if (jsModule is not null)
        {
            return jsModule.DisposeAsync();
        }
        return ValueTask.CompletedTask;
    }

    private sealed record Breadcrumb
    {
        public string Name { get; init; } = string.Empty;
        public string Path { get; init; } = string.Empty;
        public string Url { get; init; } = string.Empty;
        public bool IsLast { get; init; }
    }

    private enum ViewMode
    {
        List,
        Grid,
    }

    private enum SortField
    {
        Name,
        Size,
        Modified,
    }
}
