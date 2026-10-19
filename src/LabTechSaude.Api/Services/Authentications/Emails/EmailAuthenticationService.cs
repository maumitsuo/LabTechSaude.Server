using System.Text;
using LabTechSaude.Api.Configs;
using LabTechSaude.Api.Services.Emails.Commands;
using LabTechSaude.Application.Commands.Dispatchers;
using LabTechSaude.Application.Notifications;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace LabTechSaude.Api.Services.Authentications.Emails
{
    public class EmailAuthenticationService : IEmailAuthenticationService
    {
        private readonly Notificador _notificador;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly UrlsConfig _urlsConfig;

        public EmailAuthenticationService(
            Notificador notificador,
            ICommandDispatcher commandDispatcher,
            UserManager<IdentityUser> userManager,
            IOptions<UrlsConfig> urlsConfig)
        {
            _notificador = notificador;
            _commandDispatcher = commandDispatcher;
            _userManager = userManager;
            _urlsConfig = urlsConfig.Value;
        }

        public async Task<bool> EnviarConfirmarEmailAsync(IdentityUser user, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                _notificador.IncluirNotificacao("Não foi possível enviar o e-mail de confirmação: usuário sem endereço de e-mail.");
                return false;
            }
            
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            return await _commandDispatcher.Send<EnviarEmailCommand, bool>(
                new EnviarEmailCommand(
                    user.Email,
                    "LabTechSaude - Confirme seu endereço de e-mail",
                    CriarCorpoEmailConfirmacao(code)),
                cancellationToken);
        }

        public async Task<bool> EnviarRedefinirSenhaAsync(IdentityUser user, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_urlsConfig.FrontendUrl))
            {
                _notificador.IncluirNotificacao("Não foi possível enviar o link: URL do frontend não configurada.");
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                _notificador.IncluirNotificacao("Não foi possível enviar o link: usuário sem endereço de e-mail.");
                return false;
            }
            
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var codigo = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var link = $"{_urlsConfig.FrontendUrl.TrimEnd('/')}/redefinir-senha" +
                $"?email={Uri.EscapeDataString(user.Email)}&code={codigo}";

            return await _commandDispatcher.Send<EnviarEmailCommand, bool>(
                new EnviarEmailCommand(
                    user.Email,
                    "LabTechSaude - Redefinição de senha",
                    CriarCorpoEmailRedefinirSenha(link)),
                cancellationToken);
        }

        private static string CriarCorpoEmailConfirmacao(string code) =>
            string.Join("\r\n",
                "Olá,",
                "",
                "Use o código abaixo para confirmar seu endereço de e-mail:",
                "",
                code,
                "",
                "O código expira em 10 minutos.",
                "Se você não solicitou esta confirmação, ignore esta mensagem.");

        private static string CriarCorpoEmailRedefinirSenha(string link) =>
            string.Join("\r\n",
                "Olá,",
                "",
                "Recebemos uma solicitação para redefinir sua senha. Acesse o link abaixo:",
                "",
                link,
                "",
                "O link expira em 10 minutos.",
                "Se você não solicitou a redefinição, ignore esta mensagem.");
    }
}
