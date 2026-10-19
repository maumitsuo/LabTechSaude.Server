using LabTechSaude.Api.Models.Authentications;
using LabTechSaude.Api.Services.Authentications.Emails;
using LabTechSaude.Api.Services.Authentications.Jwt;
using LabTechSaude.Application.Commands;
using LabTechSaude.Application.Commands.Dispatchers;
using LabTechSaude.Application.Commands.Handlers;
using LabTechSaude.Application.Notifications;
using LabTechSaude.Application.Services.Usuarios.Queries;
using LabTechSaude.Application.ViewModels;
using LabTechSaude.Domain.Core.Data;
using Microsoft.AspNetCore.Identity;

namespace LabTechSaude.Api.Services.Authentications.Commands
{
    public class AuthenticationCommandHandler : BaseCommandHandler,
        ICommandHandler<EfetuarLoginCommand, LoginResponse?>,
        ICommandHandler<RenovarRefreshTokenCommand, LoginResponse?>,
        ICommandHandler<RevogarRefreshTokenCommand, bool>
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IUsuarioQuery _usuarioQuery;
        private readonly IJwtService _jwtService;
        private readonly IEmailAuthenticationService _emailAuthenticationService;

        private const string ErroPersistencia = "Ocorreu um erro ao salvar os dados no banco.";

        public AuthenticationCommandHandler(
            Notificador notificador,
            IUnitOfWork unitOfWork,
            ICommandDispatcher commandDispatcher,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IUsuarioQuery usuarioQuery,
            IJwtService jwtService,
            IEmailAuthenticationService emailAuthenticationService)
            : base(notificador, unitOfWork, commandDispatcher)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _usuarioQuery = usuarioQuery;
            _jwtService = jwtService;
            _emailAuthenticationService = emailAuthenticationService;
        }

        public async Task<LoginResponse?> Handle(EfetuarLoginCommand command, CancellationToken cancellationToken = default)
        {
            const string credenciaisInvalidas = "Usuário ou senha inválidos.";

            var user = await _userManager.FindByEmailAsync(command.Email);

            if (user == null)
            {
                NotificarErro(credenciaisInvalidas);
                return null;
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, command.Senha, lockoutOnFailure: true);

            if (result.IsLockedOut)
            {
                NotificarErro("Conta temporariamente bloqueada por excesso de tentativas. Tente novamente mais tarde.");
                return null;
            }

            if (result.IsNotAllowed)
            {
                NotificarErro("É necessário confirmar o e-mail antes de efetuar login.");
                await _emailAuthenticationService.EnviarConfirmarEmailAsync(user, cancellationToken);
                return null;
            }

            if (!result.Succeeded)
            {
                NotificarErro(credenciaisInvalidas);
                return null;
            }

            var usuario = await _usuarioQuery.ObterPorId(Guid.Parse(user.Id));

            if (usuario == null)
            {
                NotificarErro(credenciaisInvalidas);
                return null;
            }

            return await GerarLoginResponse(user, usuario, cancellationToken);
        }

        public async Task<LoginResponse?> Handle(
            RenovarRefreshTokenCommand command,
            CancellationToken cancellationToken = default)
        {
            LoginResponse? login = null;

            try
            {
                login = await _jwtService.RenovarAsync(command.RefreshToken, cancellationToken);
            }
            catch (Exception)
            {
                NotificarErro(ErroPersistencia);
            }

            return login;
        }

        public async Task<bool> Handle(
            RevogarRefreshTokenCommand command,
            CancellationToken cancellationToken = default)
        {
            var revogado = false;

            try
            {
                revogado = await _jwtService.RevogarAsync(command.RefreshToken, cancellationToken);
            }
            catch (Exception)
            {
                NotificarErro(ErroPersistencia);
            }

            return revogado;
        }

        private async Task<LoginResponse?> GerarLoginResponse(
            IdentityUser user,
            UsuarioViewModel usuario,
            CancellationToken cancellationToken)
        {
            LoginResponse? login = null;

            try
            {
                login = await _jwtService.GerarLoginResponseAsync(user, usuario, cancellationToken);
            }
            catch (Exception)
            {
                NotificarErro(ErroPersistencia);
            }

            return login;
        }
    }
}
