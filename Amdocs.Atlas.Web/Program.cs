using Amdocs.Atlas.Data;
using Amdocs.Atlas.Web;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.WindowsServices; // already present in your file

var builder = WebApplication.CreateBuilder(args);

// ✅ Run nicely as a Windows Service (no console window needed)
builder.Host.UseWindowsService(options =>
{
    options.ServiceName = "ATLAS Web";
});

// Connection string
var connectionString = builder.Configuration.GetConnectionString("AtlasDatabase");

builder.Services.AddDbContext<AtlasDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor(options =>
{
    // Configure circuit options to handle disposal exceptions gracefully
    // options.DetailedErrors = builder.Environment.IsDevelopment();
    options.DetailedErrors = true;
    options.DisconnectedCircuitMaxRetained = 100;
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
    options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
    options.MaxBufferedUnacknowledgedRenderBatches = 20;
});

builder.Services.AddSingleton<CircuitHandler, LoggingCircuitHandler>();

// ✅ Health checks (for /health)
builder.Services.AddHealthChecks();

// Configure named HttpClient for the API
// var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5186";
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://10.124.30.37:5186";

builder.Services.AddHttpClient("AtlasApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("AtlasApi"));

var app = builder.Build();

// ✅ Map Web health endpoint
app.MapHealthChecks("/health");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();