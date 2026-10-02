using LabTechSaude.Api.Configs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiConfig(builder.Configuration);
builder.Services.AddDependencyInjectionConfig(builder.Configuration);

var app = builder.Build();

app.UseApiConfiguration(app.Environment);

app.Run();
