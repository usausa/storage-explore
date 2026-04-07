using System.Runtime;
using System.Runtime.InteropServices;

using Microsoft.Extensions.Hosting.WindowsServices;

using Serilog;

using StorageExplore;
using StorageExplore.Components;
using StorageExplore.Endpoints;
using StorageExplore.Services;
using StorageExplore.Settings;

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

// Logging
builder.Logging.ClearProviders();
builder.Services.AddSerilog(options => options.ReadFrom.Configuration(builder.Configuration));

// Configuration
builder.Services.Configure<StorageSettings>(builder.Configuration.GetSection(StorageSettings.SectionName));
builder.Services.AddSingleton<FileStorageService>();

// Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// API
builder.Services.AddProblemDetails();

// Configure Kestrel for large file uploads
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 10L * 1024 * 1024 * 1024; // 10 GB
});

//--------------------------------------------------------------------------------
// Configure request pipeline
//--------------------------------------------------------------------------------

var app = builder.Build();

// Ensure the storage directories exist at startup
app.Services.GetRequiredService<FileStorageService>();

if (!app.Environment.IsDevelopment())
{
    app.UseWhen(
        static c => c.Request.Path.StartsWithSegments("/api/", StringComparison.OrdinalIgnoreCase),
        static b => b.UseExceptionHandler());
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapStaticAssets();
app.MapFileEndpoints();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Startup log
app.Logger.InfoServiceStart();
app.Logger.InfoServiceSettingsRuntime(RuntimeInformation.OSDescription, RuntimeInformation.FrameworkDescription, RuntimeInformation.RuntimeIdentifier);
var serviceVersion = typeof(Program).Assembly.GetName().Version;
app.Logger.InfoServiceSettingsEnvironment(serviceVersion, Environment.CurrentDirectory);
app.Logger.InfoServiceSettingsGC(GCSettings.IsServerGC, GCSettings.LatencyMode, GCSettings.LargeObjectHeapCompactionMode);

app.Run();
