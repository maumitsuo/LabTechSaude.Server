using LabTechSaude.Data.Context;
using LabTechSaude.Data.Repositories;
using LabTechSaude.Data.Uow;
using LabTechSaude.Domain.Core.Data;
using LabTechSaude.Domain.Pessoas;

namespace LabTechSaude.Api.Configs
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection AddDependencyInjectionConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddScoped<IPessoaRepository, PessoaRepository>();

            #region Context
            services.AddScoped<LabTechSaudeDbContext>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            #endregion Context
            return services;
        }
    }
}
