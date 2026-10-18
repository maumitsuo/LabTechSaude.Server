using LabTechSaude.Api.Context;
using LabTechSaude.Api.Services.Emails.Commands;
using LabTechSaude.Api.Services.Authentications.Commands;
using LabTechSaude.Application.Commands.Dispatchers;
using LabTechSaude.Application.Commands.Handlers;
using LabTechSaude.Application.Notifications;
using LabTechSaude.Application.Services.Usuarios.Commands;
using LabTechSaude.Application.Services.Usuarios.Queries;
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

            builder.Services.Configure<SmtpConfig>(builder.Configuration.GetSection("SmtpConfig"));

            builder.Services.AddScoped<ICommandDispatcher, CommandDispatcher>();

            builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            builder.Services.AddScoped<IUsuarioQuery, UsuarioQuery>();
            builder.Services.AddScoped<ICommandHandler<CadastrarUsuarioCommand, bool>, UsuarioCommandHandler>();
            builder.Services.AddScoped<ICommandHandler<AtualizarUsuarioCommand, bool>, UsuarioCommandHandler>();
            builder.Services.AddScoped<ICommandHandler<ExcluirUsuarioCommand, bool>, UsuarioCommandHandler>();

            builder.Services.AddScoped<ICommandHandler<CadastrarAspNetUserCommand, bool>, AuthorizationCommandHandler>();
            builder.Services.AddScoped<ICommandHandler<EnviarCodigoConfirmarEmailCommand, bool>, AuthorizationCommandHandler>();
            builder.Services.AddScoped<ICommandHandler<ConfirmarCodigoEmailCommand, bool>, AuthorizationCommandHandler>();

            builder.Services.AddScoped<ICommandHandler<EnviarEmailCommand, bool>, EmailCommandHandler>();

            builder.Services.AddScoped<Notificador>();

            #region Context
            builder.Services.AddScoped<LabTechSaudeDbContext>();
            builder.Services.AddScoped<LabTechSaudeIdentityDbContext>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            #endregion Context
            
            return builder;
        }
    }
}
