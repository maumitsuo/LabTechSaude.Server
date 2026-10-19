using System.Threading.RateLimiting;
using LabTechSaude.Api.Services.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace LabTechSaude.Api.Configs.RateLimit
{
    public static class RateLimiterConfig
    {
        public const string EnviarEmailIdentityPolicy = "email-identity";
        
        public static WebApplicationBuilder AddRateLimiterConfig(this WebApplicationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            var section = builder.Configuration.GetSection("RateLimitConfig");
            builder.Services.Configure<RateLimitConfig>(section);
            var config = section.Get<RateLimitConfig>() ?? new RateLimitConfig();

            builder.Services.AddSingleton<IRateLimitService, RedisRateLimitService>();

            builder.Services.Configure<ForwardedHeadersOptions>(options =>
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);

            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.AddPolicy(EnviarEmailIdentityPolicy, httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonimo",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = config.IpRequisicoesPorMinuto,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        }));

                options.OnRejected = (context, _) =>
                {
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                        context.HttpContext.Response.Headers.RetryAfter = ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString();

                    return ValueTask.CompletedTask;
                };
            });

            return builder;
        }
    }
}
