using LabTechSaude.Application.Notifications;
using LabTechSaude.Application.Services.Usuarios;
using LabTechSaude.Data.Context;
using LabTechSaude.Data.Repositories;
using LabTechSaude.Data.Uow;
using LabTechSaude.Domain.Core.Data;
using LabTechSaude.Domain.Usuarios;

namespace LabTechSaude.Api.Configs
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection AddDependencyInjectionConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IUsuarioService, UsuarioService>();

            services.AddScoped<Notificador>();

            #region Context
            services.AddScoped<LabTechSaudeDbContext>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            #endregion Context
            return services;
        }
    }
}
