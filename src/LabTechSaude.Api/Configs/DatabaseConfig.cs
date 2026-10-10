using LabTechSaude.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Parametriz.AutoNFP.Api.Configs
{
    public static class DatabaseConfig
    {
        public static WebApplicationBuilder AddDatabaseConfig(this WebApplicationBuilder builder)
        {
            if (builder == null) 
                throw new ArgumentNullException(nameof(builder));

            builder.Services.AddDbContext<LabTechSaudeDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            //AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            return builder;
        }
    }
}
