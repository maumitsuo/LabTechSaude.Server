using LabTechSaude.Api.Context;
using LabTechSaude.Application.Commands;
using LabTechSaude.Application.Commands.Dispatchers;
using LabTechSaude.Application.Commands.Handlers;
using LabTechSaude.Application.Notifications;
using LabTechSaude.Application.Services.Usuarios.Commands;
using LabTechSaude.Domain.Core.Data;
using Microsoft.AspNetCore.Identity;

namespace LabTechSaude.Api.Services.Authentications.Commands
{
    public class AuthorizationCommandHandler : BaseCommandHandler,
        ICommandHandler<CadastrarAspNetUserCommand>
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly LabTechSaudeIdentityDbContext _context;

        public AuthorizationCommandHandler(
            Notificador notificador,
            IUnitOfWork unitOfWork,
            ICommandDispatcher commandDispatcher,
            UserManager<IdentityUser> userManager,
            LabTechSaudeIdentityDbContext context)
            : base(notificador, unitOfWork, commandDispatcher)
        {
            _userManager = userManager;
            _context = context;
        }

        private static long ToUnixEpochDate(DateTime date) =>
            (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);

        public async Task<bool> Handle(CadastrarAspNetUserCommand command, CancellationToken cancellationToken = default)
        {
            var user = new IdentityUser 
            { 
                UserName = command.Email, 
                Email = command.Email,
            };

            if (!await CadastrarIdentityUser(user, command.Senha))
                return false;

            if (!await _commandDispatcher.Send(new CadastrarUsuarioCommand(command.Nome, command.Email, command.Cpf)))
            {
                await _userManager.DeleteAsync(user);
                return false;
            }

            await EnviarLinkConfirmarEmail(user, command.Nome);

            return true;
        }

        private async Task<bool> CadastrarIdentityUser(IdentityUser user, string senha)
        {
            var result = await _userManager.CreateAsync(user, senha);

            if (!result.Succeeded)
                return AdicionarErrosIdentity(result);
            
            return true;
        }

        private async Task EnviarLinkConfirmarEmail(IdentityUser user, string nome)
        {
            throw new NotImplementedException();
        }

        private bool AdicionarErrosIdentity(IdentityResult result)
        {
            foreach (var error in result.Errors)
                NotificarErro(error.Description);
            
            return false;
        }
    }
}
