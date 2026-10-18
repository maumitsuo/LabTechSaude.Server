using Asp.Versioning.ApiExplorer;
using LabTechSaude.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

namespace LabTechSaude.Api.Configs
{
    public static class ApiConfig
    {
        private const string CorsPolicyName = "FrontendCors";

        public static WebApplicationBuilder AddApiConfig(this WebApplicationBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.AddControllers();

            var isDevelopment = builder.Environment.IsDevelopment();
            var allowedOrigins = (builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
                .Append(builder.Configuration["UrlsConfig:FrontendUrl"] ?? string.Empty);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicyName, policy =>
                {
                    var origens = allowedOrigins
                        .Where(origem => !string.IsNullOrWhiteSpace(origem))
                        .Select(origem => origem.Trim().TrimEnd('/'))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToArray();

                    if (origens.Length > 0)
                        policy.WithOrigins(origens).AllowAnyMethod().AllowAnyHeader();
                    else if (isDevelopment)
                        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });

            builder.Services.AddApiVersionConfig(1, 0);

            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            return builder;
        }

        public static WebApplication UseApiConfiguration(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseForwardedHeaders();
            app.UseHttpsRedirection();
            app.UseCors(CorsPolicyName);
            app.UseRouting();
            app.UseRateLimiter();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            return app;
        }
    }
}
