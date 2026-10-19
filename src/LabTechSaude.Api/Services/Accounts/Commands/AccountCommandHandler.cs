using System.Text;
using LabTechSaude.Api.Configs;
using LabTechSaude.Api.Services.Authentications.Emails;
using LabTechSaude.Api.Services.Accounts.RateLimiting;
using LabTechSaude.Application.Commands;
using LabTechSaude.Application.Commands.Dispatchers;
using LabTechSaude.Application.Commands.Handlers;
using LabTechSaude.Application.Notifications;
using LabTechSaude.Application.Services.Usuarios.Commands;
using LabTechSaude.Domain.Core.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using LabTechSaude.Api.Services.Authentications.Jwt;

namespace LabTechSaude.Api.Services.Accounts.Commands
{
    public class AccountCommandHandler : BaseCommandHandler,
        ICommandHandler<CadastrarAspNetUserCommand, bool>,
        ICommandHandler<EnviarConfirmarEmailCommand, bool>,
        ICommandHandler<ConfirmarEmailCommand, bool>,
        ICommandHandler<EsqueciMinhaSenhaCommand, bool>,
        ICommandHandler<RedefinirSenhaCommand, bool>
    {
        private readonly UrlsConfig _urlsConfig;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IRateLimitAuthenticationService _rateLimitAuthenticationService;
        private readonly IJwtService _jwtService;
        private readonly IEmailAuthenticationService _emailAuthenticationService;

        private const string ErroPersistencia = "Ocorreu um erro ao salvar os dados no banco.";

        public AccountCommandHandler(
            Notificador notificador,
            IUnitOfWork unitOfWork,
            ICommandDispatcher commandDispatcher,
            UserManager<IdentityUser> userManager,
            IRateLimitAuthenticationService rateLimitAuthenticationService,
            IOptions<UrlsConfig> urlsConfig,
            IJwtService jwtService,
            IEmailAuthenticationService emailAuthenticationService)
            : base(notificador, unitOfWork, commandDispatcher)
        {
            _urlsConfig = urlsConfig.Value;
            _userManager = userManager;
            _rateLimitAuthenticationService = rateLimitAuthenticationService;
            _jwtService = jwtService;
            _emailAuthenticationService = emailAuthenticationService;
        }

        public async Task<bool> Handle(CadastrarAspNetUserCommand command, CancellationToken cancellationToken = default)
        {
            var user = new IdentityUser 
            { 
                UserName = command.Email, 
                Email = command.Email,
            };

            if (!await CadastrarIdentityUser(user, command.Senha))
                return false;

            if (!await _commandDispatcher.Send<CadastrarUsuarioCommand, bool>(
                new CadastrarUsuarioCommand(Guid.Parse(user.Id), command.Nome, command.Email, command.Cpf)))
            {
                await _userManager.DeleteAsync(user);
                return false;
            }

            if (!await _emailAuthenticationService.EnviarConfirmarEmailAsync(user, cancellationToken))
                return false;

            return CommandEhValido();
        }

        public async Task<bool> Handle(EnviarConfirmarEmailCommand command, CancellationToken cancellationToken = default)
        {
            if (!await _rateLimitAuthenticationService.PermitirEnvioCodigoAsync(
                "envio-confirmar",
                command.Email,
                "Aguarde antes de solicitar um novo código.",
                "Limite de solicitações de código excedido."))
                return false;

            var user = await _userManager.FindByEmailAsync(command.Email);

            if (user == null)
                return NotificarErro("Requisição inválida.");

            if (user.EmailConfirmed)
                return NotificarErro("Conta de e-mail já confirmada.");

            if (!await _emailAuthenticationService.EnviarConfirmarEmailAsync(user, cancellationToken))
                return false;

            return CommandEhValido();
        }

        public async Task<bool> Handle(ConfirmarEmailCommand command, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(command.Email);

            if (user == null)
                return NotificarErro("Requisição inválida.");

            if (user.EmailConfirmed)
                return NotificarErro("Conta de e-mail já confirmada.");

            var result = await _userManager.ConfirmEmailAsync(user, command.Code);

            if (!result.Succeeded)
                return AdicionarErrosIdentity(result);
            
            return CommandEhValido();
        }

        public async Task<bool> Handle(EsqueciMinhaSenhaCommand command, CancellationToken cancellationToken = default)
        {
            if (!await _rateLimitAuthenticationService.PermitirEnvioCodigoAsync(
                "esqueci-senha",
                command.Email,
                "Aguarde antes de solicitar um novo link.",
                "Limite de solicitações de redefinição de senha excedido."))
                return false;

            if (string.IsNullOrWhiteSpace(_urlsConfig.FrontendUrl))
                return NotificarErro("Não foi possível enviar o link: URL do frontend não configurada.");

            var user = await _userManager.FindByEmailAsync(command.Email);

            // Resposta idêntica para e-mails inexistentes ou não confirmados, evitando enumeração de usuários.
            if (user == null || string.IsNullOrWhiteSpace(user.Email) || !user.EmailConfirmed)
                return true;

            if (!await _emailAuthenticationService.EnviarRedefinirSenhaAsync(user, cancellationToken))
                return false;

            return CommandEhValido();
        }

        public async Task<bool> Handle(RedefinirSenhaCommand command, CancellationToken cancellationToken = default)
        {
            const string requisicaoInvalida = "Requisição inválida.";

            var user = await _userManager.FindByEmailAsync(command.Email);

            if (user == null)
                return NotificarErro(requisicaoInvalida);

            string code;

            try
            {
                code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(command.Codigo));
            }
            catch (FormatException)
            {
                return NotificarErro("Código de redefinição inválido ou expirado.");
            }

            var result = await _userManager.ResetPasswordAsync(user, code, command.Senha);

            if (!result.Succeeded)
                return AdicionarErrosIdentity(result);

            try
            {
                await _jwtService.RevogarSessoesDoUsuarioAsync(user.Id, cancellationToken);
            }
            catch (Exception)
            {
                return NotificarErro(ErroPersistencia);
            }

            return CommandEhValido();
        }

        private async Task<bool> CadastrarIdentityUser(IdentityUser user, string senha)
        {
            var result = await _userManager.CreateAsync(user, senha);

            if (!result.Succeeded)
                return AdicionarErrosIdentity(result);
            
            return true;
        }

        private bool AdicionarErrosIdentity(IdentityResult result)
        {
            foreach (var error in result.Errors)
                NotificarErro(error.Description);

            return false;
        }
    }
}
