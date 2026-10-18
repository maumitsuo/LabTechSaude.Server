using LabTechSaude.Application.Commands;
using LabTechSaude.Application.Commands.Dispatchers;
using LabTechSaude.Application.Commands.Handlers;
using LabTechSaude.Application.Notifications;
using LabTechSaude.Application.Services.Usuarios.Commands;
using LabTechSaude.Domain.Core.Data;
using Microsoft.AspNetCore.Identity;
using LabTechSaude.Api.Services.Authentications;
using LabTechSaude.Api.Services.Emails.Commands;

namespace LabTechSaude.Api.Services.Authentications.Commands
{
    public class AuthorizationCommandHandler : BaseCommandHandler,
        ICommandHandler<CadastrarAspNetUserCommand, bool>,
        ICommandHandler<EnviarCodigoConfirmarEmailCommand, bool>,
        ICommandHandler<ConfirmarCodigoEmailCommand, bool>
    {
        private readonly UserManager<IdentityUser> _userManager;

        public AuthorizationCommandHandler(
            Notificador notificador,
            IUnitOfWork unitOfWork,
            ICommandDispatcher commandDispatcher,
            UserManager<IdentityUser> userManager)
            : base(notificador, unitOfWork, commandDispatcher)
        {
            _userManager = userManager;
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
                new CadastrarUsuarioCommand(command.Nome, command.Email, command.Cpf)))
            {
                await _userManager.DeleteAsync(user);
                return false;
            }

            if (!await EnviarCodigoConfirmarEmail(user, cancellationToken))
                return false;

            return CommandEhValido();
        }

        private async Task<bool> CadastrarIdentityUser(IdentityUser user, string senha)
        {
            var result = await _userManager.CreateAsync(user, senha);

            if (!result.Succeeded)
                return AdicionarErrosIdentity(result);
            
            return true;
        }

        private async Task<bool> EnviarCodigoConfirmarEmail(IdentityUser user, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
                return NotificarErro("Não foi possível enviar o e-mail de confirmação: usuário sem endereço de e-mail.");

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var command = new EnviarEmailCommand(
                user.Email,
                "LabTechSaude - Confirme seu endereço de e-mail",
                CriarCorpoEmailConfirmacao(code));

            return await _commandDispatcher.Send<EnviarEmailCommand, bool>(command, cancellationToken);
        }

        public async Task<bool> Handle(EnviarCodigoConfirmarEmailCommand command, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(command.Email);

            if (user == null)
                return NotificarErro("Requisição inválida.");

            if (user.EmailConfirmed)
                return NotificarErro("Conta de e-mail já confirmada.");

            if (!await EnviarCodigoConfirmarEmail(user, cancellationToken))
                return false;

            return CommandEhValido();
        }

        public async Task<bool> Handle(ConfirmarCodigoEmailCommand command, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(command.Email);

            if (user == null)
                return NotificarErro("Requisição inválida.");

            if (user.EmailConfirmed)
                return NotificarErro("Conta de e-mail já confirmada.");

            var result = await _userManager.ConfirmEmailAsync(user, command.Code);

            if (!result.Succeeded)
                return AdicionarErrosIdentity(result);
            
            return true;
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

        private bool AdicionarErrosIdentity(IdentityResult result)
        {
            foreach (var error in result.Errors)
                NotificarErro(error.Description);

            return false;
        }
    }
}
