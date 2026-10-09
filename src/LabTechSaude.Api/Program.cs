using LabTechSaude.Api.Configs;
using Parametriz.AutoNFP.Api.Configs;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", true, true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
    .AddEnvironmentVariables();

builder
    .AddDatabaseConfig()
    .AddApiConfig()
    .AddDependencyInjectionConfig();

var app = builder.Build();

app.UseApiConfiguration(app.Environment);

app.Run();
