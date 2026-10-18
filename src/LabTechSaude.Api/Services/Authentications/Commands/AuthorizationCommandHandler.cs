using LabTechSaude.Api.Configs;
using LabTechSaude.Application.Commands;
using Microsoft.AspNetCore.WebUtilities;
using LabTechSaude.Application.Commands.Dispatchers;
using LabTechSaude.Application.Commands.Handlers;
using LabTechSaude.Application.Notifications;
using LabTechSaude.Application.Services.Usuarios.Commands;
using LabTechSaude.Domain.Core.Data;
using Microsoft.AspNetCore.Identity;
using LabTechSaude.Api.Configs.Identity;
using LabTechSaude.Application.Services.Usuarios.Queries;
using Microsoft.Extensions.Options;
using System.Text;
using LabTechSaude.Api.Services.Emails.Commands;
using LabTechSaude.Api.Models.Authentications;
using LabTechSaude.Application.ViewModels;
using LabTechSaude.Api.Services.RateLimiting;
using System.Security.Cryptography;
using LabTechSaude.Api.Configs.RateLimit;
using LabTechSaude.Api.Services.Authentications.Jwt;

namespace LabTechSaude.Api.Services.Authentications.Commands
{
    public class AuthorizationCommandHandler : BaseCommandHandler,
        ICommandHandler<CadastrarAspNetUserCommand, bool>,
        ICommandHandler<EnviarConfirmarEmailCommand, bool>,
        ICommandHandler<ConfirmarEmailCommand, bool>,
        ICommandHandler<EsqueciMinhaSenhaCommand, bool>,
        ICommandHandler<RedefinirSenhaCommand, bool>,
        ICommandHandler<EfetuarLoginCommand, LoginResponse?>,
        ICommandHandler<RenovarRefreshTokenCommand, LoginResponse?>,
        ICommandHandler<RevogarRefreshTokenCommand, bool>
    {
        private readonly UrlsConfig _urlsConfig;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IUsuarioQuery _usuarioQuery;
        private readonly IRateLimitService _rateLimitService;
        private readonly RateLimitConfig _rateLimitConfig;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtService _jwtService;

        private const string ErroPersistencia = "Ocorreu um erro ao salvar os dados no banco.";

        public AuthorizationCommandHandler(
            Notificador notificador,
            IUnitOfWork unitOfWork,
            ICommandDispatcher commandDispatcher,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IUsuarioQuery usuarioQuery,
            IRateLimitService rateLimitService,
            IOptions<RateLimitConfig> rateLimitConfig,
            IHttpContextAccessor httpContextAccessor,
            IOptions<UrlsConfig> urlsConfig,
            IJwtService jwtService)
            : base(notificador, unitOfWork, commandDispatcher)
        {
            _urlsConfig = urlsConfig.Value;
            _userManager = userManager;
            _signInManager = signInManager;
            _usuarioQuery = usuarioQuery;
            _rateLimitService = rateLimitService;
            _rateLimitConfig = rateLimitConfig.Value;
            _httpContextAccessor = httpContextAccessor;
            _jwtService = jwtService;
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
                await EnviarConfirmarEmail(user, cancellationToken);
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

            if (!await EnviarConfirmarEmail(user, cancellationToken))
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

        private async Task<bool> EnviarConfirmarEmail(IdentityUser user, CancellationToken cancellationToken)
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

        public async Task<bool> Handle(EnviarConfirmarEmailCommand command, CancellationToken cancellationToken = default)
        {
            var chave = ChaveEmail("envio-confirmar", command.Email);

            var cooldown = await _rateLimitService.TentarConsumirAsync(
                chave + ":cooldown", 1, TimeSpan.FromSeconds(_rateLimitConfig.EnvioCodigoCooldownSegundos), TimeSpan.Zero);

            if (!cooldown.Permitido)
                return NotificarLimiteExcedido("Aguarde antes de solicitar um novo código.", cooldown.RetryAfter);

            var limiteHora = await _rateLimitService.TentarConsumirAsync(
                chave,
                _rateLimitConfig.EnvioCodigoMaximoPorHora,
                TimeSpan.FromHours(1),
                TimeSpan.FromMinutes(_rateLimitConfig.EnvioCodigoBloqueioMinutos));

            if (!limiteHora.Permitido)
                return NotificarLimiteExcedido("Limite de solicitações de código excedido.", limiteHora.RetryAfter);

            var user = await _userManager.FindByEmailAsync(command.Email);

            if (user == null)
                return NotificarErro("Requisição inválida.");

            if (user.EmailConfirmed)
                return NotificarErro("Conta de e-mail já confirmada.");

            if (!await EnviarConfirmarEmail(user, cancellationToken))
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
            var chave = ChaveEmail("esqueci-senha", command.Email);

            var cooldown = await _rateLimitService.TentarConsumirAsync(
                chave + ":cooldown", 1, TimeSpan.FromSeconds(_rateLimitConfig.EnvioCodigoCooldownSegundos), TimeSpan.Zero);

            if (!cooldown.Permitido)
                return NotificarLimiteExcedido("Aguarde antes de solicitar um novo link.", cooldown.RetryAfter);

            var limiteHora = await _rateLimitService.TentarConsumirAsync(
                chave,
                _rateLimitConfig.EnvioCodigoMaximoPorHora,
                TimeSpan.FromHours(1),
                TimeSpan.FromMinutes(_rateLimitConfig.EnvioCodigoBloqueioMinutos));

            if (!limiteHora.Permitido)
                return NotificarLimiteExcedido("Limite de solicitações de redefinição de senha excedido.", limiteHora.RetryAfter);

            if (string.IsNullOrWhiteSpace(_urlsConfig.FrontendUrl))
                return NotificarErro("Não foi possível enviar o link: URL do frontend não configurada.");

            var user = await _userManager.FindByEmailAsync(command.Email);

            // Resposta idêntica para e-mails inexistentes ou não confirmados, evitando enumeração de usuários.
            if (user == null || string.IsNullOrWhiteSpace(user.Email) || !user.EmailConfirmed)
                return true;

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var codigo = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var link = $"{_urlsConfig.FrontendUrl.TrimEnd('/')}/redefinir-senha" +
                $"?email={Uri.EscapeDataString(user.Email)}&code={codigo}";

            var emailCommand = new EnviarEmailCommand(
                user.Email,
                "LabTechSaude - Redefinição de senha",
                CriarCorpoEmailRedefinirSenha(link));

            if (!await _commandDispatcher.Send<EnviarEmailCommand, bool>(emailCommand, cancellationToken))
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

        private static string ChaveEmail(string operacao, string email)
        {
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(email.Trim().ToLowerInvariant())));
            return $"{operacao}:{hash}";
        }

        private bool NotificarLimiteExcedido(string mensagem, TimeSpan retryAfter)
        {
            var segundos = (int)Math.Ceiling(retryAfter.TotalSeconds);

            if (_httpContextAccessor.HttpContext is { } httpContext)
                httpContext.Items[RateLimitHttpItems.RetryAfterSegundos] = segundos;

            return NotificarErro($"{mensagem} Tente novamente em {segundos} segundos.");
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

        private bool AdicionarErrosIdentity(IdentityResult result)
        {
            foreach (var error in result.Errors)
                NotificarErro(error.Description);

            return false;
        }
    }
}
