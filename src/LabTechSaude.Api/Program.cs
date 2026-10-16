using LabTechSaude.Api.Configs;
using LabTechSaude.Api.Configs.Identity;
using Parametriz.AutoNFP.Api.Configs;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", true, true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
    .AddEnvironmentVariables();

builder
    .AddDatabaseConfig()
    .AddIdentityConfig()
    .AddApiConfig()
    .AddDependencyInjectionConfig()
    .AddLabTechSaudeDocsConfig();

var app = builder.Build();

app
    .UseLabTechSaudeDocs()
    .UseApiConfiguration();

app.Run();
