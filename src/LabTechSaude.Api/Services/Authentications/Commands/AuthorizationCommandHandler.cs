using LabTechSaude.Application.Commands;
using LabTechSaude.Application.Commands.Dispatchers;
using LabTechSaude.Application.Commands.Handlers;
using LabTechSaude.Application.Notifications;
using LabTechSaude.Application.Services.Usuarios.Commands;
using LabTechSaude.Domain.Core.Data;
using Microsoft.AspNetCore.Identity;
using LabTechSaude.Api.Configs.Identity;
using LabTechSaude.Application.Services.Usuarios.Queries;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LabTechSaude.Api.Services.Authentications;
using LabTechSaude.Api.Services.Emails.Commands;
using LabTechSaude.Api.Models.Authentications;
using LabTechSaude.Application.ViewModels;
using LabTechSaude.Api.Services.RateLimiting;
using System.Security.Cryptography;
using LabTechSaude.Api.Configs.RateLimit;

namespace LabTechSaude.Api.Services.Authentications.Commands
{
    public class AuthorizationCommandHandler : BaseCommandHandler,
        ICommandHandler<CadastrarAspNetUserCommand, bool>,
        ICommandHandler<EnviarCodigoConfirmarEmailCommand, bool>,
        ICommandHandler<ConfirmarCodigoEmailCommand, bool>,
        ICommandHandler<EfetuarLoginCommand, LoginResponse?>
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IUsuarioQuery _usuarioQuery;
        private readonly JwtTokenOptions _jwtTokenOptions;
        private readonly IRateLimitService _rateLimitService;
        private readonly RateLimitConfig _rateLimitConfig;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthorizationCommandHandler(
            Notificador notificador,
            IUnitOfWork unitOfWork,
            ICommandDispatcher commandDispatcher,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IUsuarioQuery usuarioQuery,
            IOptions<JwtTokenOptions> jwtTokenOptions,
            IRateLimitService rateLimitService,
            IOptions<RateLimitConfig> rateLimitConfig,
            IHttpContextAccessor httpContextAccessor)
            : base(notificador, unitOfWork, commandDispatcher)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _usuarioQuery = usuarioQuery;
            _jwtTokenOptions = jwtTokenOptions.Value;
            _rateLimitService = rateLimitService;
            _rateLimitConfig = rateLimitConfig.Value;
            _httpContextAccessor = httpContextAccessor;
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

            return await GerarLoginResponse(user, usuario);
        }

        private async Task<LoginResponse?> GerarLoginResponse(IdentityUser user, UsuarioViewModel usuario)
        {
            var notBefore = DateTime.Now;
            var expires = notBefore.AddHours(1);

            return new LoginResponse
            {
                AccessToken = await GerarAccessToken(user, notBefore, expires),
                NotBefore = notBefore,
                Expires = expires,
                Usuario = new UsuarioResponse
                {
                    Id = usuario.Id,
                    Nome = usuario.Nome,
                    Email = usuario.Email
                }
            };
        }

        private async Task<string> GerarAccessToken(IdentityUser user, DateTime notBefore, DateTime expires)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in await _userManager.GetRolesAsync(user))
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtTokenOptions.SecretKey));

            var token = new JwtSecurityToken(
                issuer: _jwtTokenOptions.Issuer,
                audience: _jwtTokenOptions.Audience,
                claims: claims,
                notBefore: notBefore,
                expires: expires,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
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
            var chave = ChaveEmail("envio", command.Email);

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

            if (!await EnviarCodigoConfirmarEmail(user, cancellationToken))
                return false;

            return CommandEhValido();
        }

        public async Task<bool> Handle(ConfirmarCodigoEmailCommand command, CancellationToken cancellationToken = default)
        {
            var chave = ChaveEmail("confirmacao", command.Email);

            var bloqueio = await _rateLimitService.ConsultarBloqueioAsync(chave);

            if (!bloqueio.Permitido)
                return NotificarLimiteExcedido("Muitas tentativas de confirmação.", bloqueio.RetryAfter);

            var user = await _userManager.FindByEmailAsync(command.Email);

            if (user == null)
                return NotificarErro("Requisição inválida.");

            if (user.EmailConfirmed)
                return NotificarErro("Conta de e-mail já confirmada.");

            var result = await _userManager.ConfirmEmailAsync(user, command.Code);

            if (!result.Succeeded)
            {
                await _rateLimitService.TentarConsumirAsync(
                    chave,
                    _rateLimitConfig.ConfirmacaoCodigoMaximoFalhas,
                    TimeSpan.FromMinutes(_rateLimitConfig.ConfirmacaoCodigoJanelaMinutos),
                    TimeSpan.FromMinutes(_rateLimitConfig.ConfirmacaoCodigoBloqueioMinutos));

                return AdicionarErrosIdentity(result);
            }

            await _rateLimitService.ResetarAsync(chave);

            return true;
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

        private bool AdicionarErrosIdentity(IdentityResult result)
        {
            foreach (var error in result.Errors)
                NotificarErro(error.Description);

            return false;
        }
    }
}
