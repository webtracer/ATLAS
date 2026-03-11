using Amdocs.Atlas.Data;
using Microsoft.EntityFrameworkCore;
using Amdocs.Atlas.Api.Mapping;
using Amdocs.Atlas.Core.Interfaces;
using Amdocs.Atlas.Data.Repositories;
using Amdocs.Atlas.Api.Services;
using Microsoft.Extensions.Hosting.WindowsServices;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:5186");

builder.Host.UseWindowsService(options =>
{
    options.ServiceName = "ATLAS API";
});

// Connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("AtlasDatabase");

// DbContext (only once)
builder.Services.AddDbContext<AtlasDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Repositories
builder.Services.AddScoped<IServerRepository, ServerRepository>();

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// AutoMapper
builder.Services.AddAutoMapper(typeof(AtlasProfile));

// Register vSphere Analysis services
builder.Services.AddScoped<IVSphereClientService, VSphereClientService>();
builder.Services.AddScoped<IAIAnalysisService, AIAnalysisService>();
builder.Services.AddScoped<IVSphereAnalysisService, VSphereAnalysisService>();

// Register HttpClient for CoPilot/AI services
builder.Services.AddHttpClient("CoPilot", client =>
{
    var endpoint = builder.Configuration["CoPilot:Endpoint"] ?? "https://api.openai.com/v1/chat/completions";
    client.BaseAddress = new Uri(endpoint);
    client.Timeout = TimeSpan.FromMinutes(5);
});

// Register HttpClientFactory for vSphere REST API calls
builder.Services.AddHttpClient();

// ✅ Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Swagger in dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

// ✅ Map health endpoint
app.MapHealthChecks("/health");

app.MapControllers();

app.Run();