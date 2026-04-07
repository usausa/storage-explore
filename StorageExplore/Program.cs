using System.Runtime;
using System.Runtime.InteropServices;

using Microsoft.Extensions.Hosting.WindowsServices;

using Serilog;

using StorageExplore;
using StorageExplore.Application;
using StorageExplore.Components;
using StorageExplore.Endpoints;
using StorageExplore.Services;

//--------------------------------------------------------------------------------
// Configure builder
//--------------------------------------------------------------------------------
Directory.SetCurrentDirectory(AppContext.BaseDirectory);
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : default
});

// Path
builder.Configuration.SetBasePath(AppContext.BaseDirectory);

// Service
builder.Host
    .UseWindowsService()
    .UseSystemd();

// Allow large file uploads
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 10L * 1024 * 1024 * 1024; // 10 GB
});

// Logging
builder.Logging.ClearProviders();
builder.Services.AddSerilog(options => options.ReadFrom.Configuration(builder.Configuration));

// Storage service
builder.Services.Configure<FileStorageSetting>(builder.Configuration.GetSection("Storage"));
builder.Services.AddSingleton<FileStorageService>();

// Blazor Server
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// API
builder.Services.AddProblemDetails();

//--------------------------------------------------------------------------------
// Configure request pipeline
//--------------------------------------------------------------------------------

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseWhen(
        static c => c.Request.Path.StartsWithSegments("/api/", StringComparison.OrdinalIgnoreCase),
        static b => b.UseExceptionHandler(),
        static b => b.UseExceptionHandler("/error", createScopeForErrors: true));
}

// End point
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

// End point
app.MapStaticAssets();

// File API
app.MapFileEndpoints();

// Blazor pages
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Initialize storage service
app.Services.GetRequiredService<FileStorageService>().Initialize();

// Startup log
app.Logger.InfoServiceStart();
app.Logger.InfoServiceSettingsRuntime(RuntimeInformation.OSDescription, RuntimeInformation.FrameworkDescription, RuntimeInformation.RuntimeIdentifier);
app.Logger.InfoServiceSettingsEnvironment(typeof(Program).Assembly.GetName().Version, Environment.CurrentDirectory);
app.Logger.InfoServiceSettingsGC(GCSettings.IsServerGC, GCSettings.LatencyMode, GCSettings.LargeObjectHeapCompactionMode);

app.Run();
