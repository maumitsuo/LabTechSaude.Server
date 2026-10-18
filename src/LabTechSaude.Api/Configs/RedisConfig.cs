namespace LabTechSaude.Api.Configs
{
    public static class RedisConfig
    {
        public static WebApplicationBuilder AddRedisConfig(this WebApplicationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            var connectionString = builder.Configuration.GetConnectionString("Redis");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("ConnectionStrings:Redis não configurada.");

            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = connectionString;
                options.InstanceName = "LabTechSaude:";
            });

            return builder;
        }
    }
}
