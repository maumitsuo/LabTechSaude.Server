using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

namespace LabTechSaude.Api.Configs
{
    public static class DocsConfig
    {
        public static readonly string[] ApiDocumentNames = ["v1"];
        public static WebApplicationBuilder AddDocsConfig(this WebApplicationBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.AddTransient<IConfigureOptions<OpenApiOptions>, LabTechSaudeOpenApiOptions>();

            foreach (var name in ApiDocumentNames)
            {
                builder.Services.AddOpenApi(name);
            }
            
            return builder;
        }

        public class LabTechSaudeOpenApiOptions : IConfigureNamedOptions<OpenApiOptions>
        {
            private readonly IApiVersionDescriptionProvider _provider;

            public LabTechSaudeOpenApiOptions(
                IApiVersionDescriptionProvider provider)
            {
                _provider = provider;
            }

            public void Configure(OpenApiOptions options)
            {
                var defaultGroupName = _provider.ApiVersionDescriptions
                    .FirstOrDefault()?.GroupName;

                Configure(defaultGroupName, options);
            }

            public void Configure(string? name, OpenApiOptions options)
            {
                var description = _provider.ApiVersionDescriptions
                    .SingleOrDefault(d => d.GroupName == name);

                if (description is null)
                {
                    throw new InvalidOperationException(
                        $"Não foi encontrada uma descrição para o documento OpenAPI '{name}'.");
                }

                options.ShouldInclude = apiDescription =>
                    apiDescription.GroupName == description.GroupName;
                options.AddDocumentTransformer(new LabTechSaudeDocumentTransformer(description));
                options.AddOperationTransformer(new LabTechSaudeOperationTransform());
            }
        }

        public class LabTechSaudeDocumentTransformer : IOpenApiDocumentTransformer
        {
            public ApiVersionDescription Description { get; }

            public LabTechSaudeDocumentTransformer(ApiVersionDescription description)
            {
                Description = description;
            }

            public Task TransformAsync(
                OpenApiDocument document, 
                OpenApiDocumentTransformerContext context, 
                CancellationToken cancellationToken)
            {
                // Information
                document.Info.Title = "LabTechSaude API";
                document.Info.Version = Description.GroupName;
                document.Info.Description = $"API LabTechSaude App {Description.GroupName.ToUpper()}.";
                document.Info.Contact = new OpenApiContact { Name = "Mauricio Mitsuo Kamikoga", Email = "mau.mitsuo@gmail.com" };
                document.Info.TermsOfService = new Uri("https://opensource.org/licenses/MIT");
                document.Info.License = new OpenApiLicense { Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT") };
                
                if (Description.IsDeprecated)
                {
                    document.Info.Description += " Esta versão está obsoleta.";
                }

                // Security
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
                {
                    ["Bearer"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        In = ParameterLocation.Header,
                        BearerFormat = "JWT",
                        Description = "Insira o token JWT",
                        Name = "Authorization"
                    }
                };

                foreach(var path in document.Paths.Values)
                {
                    if (!(path.Operations?.Values?.Any() ?? false))
                        continue;

                    foreach(var operation in path.Operations.Values)
                    {
                        operation.Security ??= [];
                        operation.Security.Add(new OpenApiSecurityRequirement
                        {
                            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                        });
                    }
                }

                return Task.CompletedTask;
            }
        }

        public class LabTechSaudeOperationTransform : IOpenApiOperationTransformer
        {
            public Task TransformAsync(
                OpenApiOperation operation,
                OpenApiOperationTransformerContext context,
                CancellationToken cancellationToken)
            {
                var apiDescription = context.Description;

                operation.Deprecated = apiDescription.IsDeprecated;

                if (!(operation.Parameters?.Any() ?? false))
                    return Task.CompletedTask;

                foreach (var parameter in operation.Parameters!)
                {
                    var description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);

                    if (parameter.Description == null)
                    {
                        parameter.Description = description.ModelMetadata?.Description;
                    }
                }

                return Task.CompletedTask;
            }
        }
    }
}
