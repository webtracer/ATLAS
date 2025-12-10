using Amdocs.Atlas.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Connection string
var connectionString = builder.Configuration.GetConnectionString("AtlasDatabase");

builder.Services.AddDbContext<AtlasDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

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