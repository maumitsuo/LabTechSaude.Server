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
using LabTechSaude.Api.Models.Authentications;
using LabTechSaude.Api.Services.Authentications.RefreshTokens;
using LabTechSaude.Api.Services.Authentications.RefreshTokens.Repository;
using LabTechSaude.Api.Services.Authentications.Jwt;

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
            builder.Services.Configure<UrlsConfig>(builder.Configuration.GetSection("UrlsConfig"));

            builder.Services.AddScoped<ICommandDispatcher, CommandDispatcher>();

            builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            builder.Services.AddScoped<IUsuarioQuery, UsuarioQuery>();
            builder.Services.AddScoped<ICommandHandler<CadastrarUsuarioCommand, bool>, UsuarioCommandHandler>();
            builder.Services.AddScoped<ICommandHandler<AtualizarUsuarioCommand, bool>, UsuarioCommandHandler>();
            builder.Services.AddScoped<ICommandHandler<ExcluirUsuarioCommand, bool>, UsuarioCommandHandler>();

            builder.Services.AddScoped<ICommandHandler<CadastrarAspNetUserCommand, bool>, AuthorizationCommandHandler>();
            builder.Services.AddScoped<ICommandHandler<EnviarConfirmarEmailCommand, bool>, AuthorizationCommandHandler>();
            builder.Services.AddScoped<ICommandHandler<ConfirmarEmailCommand, bool>, AuthorizationCommandHandler>();
            builder.Services.AddScoped<ICommandHandler<EsqueciMinhaSenhaCommand, bool>, AuthorizationCommandHandler>();
            builder.Services.AddScoped<ICommandHandler<RedefinirSenhaCommand, bool>, AuthorizationCommandHandler>();

            builder.Services.AddScoped<ICommandHandler<EfetuarLoginCommand, LoginResponse?>, AuthorizationCommandHandler>();
            builder.Services.AddScoped<ICommandHandler<RenovarRefreshTokenCommand, LoginResponse?>, AuthorizationCommandHandler>();
            builder.Services.AddScoped<ICommandHandler<RevogarRefreshTokenCommand, bool>, AuthorizationCommandHandler>();

            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

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
