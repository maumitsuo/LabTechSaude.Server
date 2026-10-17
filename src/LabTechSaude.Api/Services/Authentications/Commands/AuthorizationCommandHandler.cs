using LabTechSaude.Api.Context;
using LabTechSaude.Application.Commands;
using LabTechSaude.Application.Commands.Dispatchers;
using LabTechSaude.Application.Commands.Handlers;
using LabTechSaude.Application.Notifications;
using LabTechSaude.Application.Services.Usuarios.Commands;
using LabTechSaude.Domain.Core.Data;
using LabTechSaude.Domain.Usuarios;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace LabTechSaude.Api.Services.Authentications.Commands
{
    public class AuthorizationCommandHandler : BaseCommandHandler,
        ICommandHandler<CadastrarAspNetUserCommand>,
        ICommandHandler<EnviarCodigoConfirmarEmailCommand>,
        ICommandHandler<ConfirmarEmailCommand>
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly LabTechSaudeIdentityDbContext _context;
        private readonly IUsuarioRepository _usuarioRepository;
        public AuthorizationCommandHandler(
            Notificador notificador,
            IUnitOfWork unitOfWork,
            ICommandDispatcher commandDispatcher,
            UserManager<IdentityUser> userManager,
            LabTechSaudeIdentityDbContext context,
            IUsuarioRepository usuarioRepository)
            : base(notificador, unitOfWork, commandDispatcher)
        {
            _userManager = userManager;
            _context = context;
            _usuarioRepository = usuarioRepository;
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

            await EnviarLinkConfirmarEmail(user);

            return CommandEhValido();
        }

        private async Task<bool> CadastrarIdentityUser(IdentityUser user, string senha)
        {
            var result = await _userManager.CreateAsync(user, senha);

            if (!result.Succeeded)
                return AdicionarErrosIdentity(result);
            
            return true;
        }

        private async Task EnviarLinkConfirmarEmail(IdentityUser user)
        {
            var usuario = await _usuarioRepository.ObterPorId(Guid.Parse(user.Id));


        }

        public async Task<bool> Handle(EnviarCodigoConfirmarEmailCommand command, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(command.UsuarioId.ToString());

            if (user == null)
                return NotificarErro("Requisição inválida.");

            if (user.EmailConfirmed)
                return NotificarErro("Conta de e-mail já confirmada.");

            await EnviarLinkConfirmarEmail(user);

            return CommandEhValido();
        }

        public async Task<bool> Handle(ConfirmarEmailCommand command, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(command.Email);

            if (user == null)
                return NotificarErro("Requisição inválida.");

            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(command.Code));
            var result = await _userManager.ConfirmEmailAsync(user, code);

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
