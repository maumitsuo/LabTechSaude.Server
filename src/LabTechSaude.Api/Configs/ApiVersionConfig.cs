using Asp.Versioning;

namespace LabTechSaude.Api.Configs
{
    public static class ApiVersionConfig
    {
        public static IServiceCollection AddApiVersionConfig(this IServiceCollection services, int majorVersion, int minorVersion)
        {
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(majorVersion, minorVersion);
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            return services;
        }
    }
}
