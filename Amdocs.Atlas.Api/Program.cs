using Amdocs.Atlas.Data;
using Microsoft.EntityFrameworkCore;
using Amdocs.Atlas.Api.Mapping;
using Amdocs.Atlas.Core.Interfaces;
using Amdocs.Atlas.Data.Repositories;


var builder = WebApplication.CreateBuilder(args);

// Connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("AtlasDatabase");

// DbContext
builder.Services.AddDbContext<AtlasDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddDbContext<AtlasDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 👇 Add this
builder.Services.AddScoped<IServerRepository, ServerRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(AtlasProfile));

var app = builder.Build();

// Swagger in dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();