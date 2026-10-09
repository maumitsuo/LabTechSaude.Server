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
        public static WebApplicationBuilder AddDependencyInjectionConfig(this WebApplicationBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            builder.Services.AddScoped<IUsuarioService, UsuarioService>();

            builder.Services.AddScoped<Notificador>();

            #region Context
            builder.Services.AddScoped<LabTechSaudeDbContext>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            #endregion Context
            
            return builder;
        }
    }
}
