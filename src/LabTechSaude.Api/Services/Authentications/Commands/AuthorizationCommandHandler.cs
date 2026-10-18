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
using LabTechSaude.Api.Context;
using Microsoft.EntityFrameworkCore;

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
        private readonly UrlsConfig _frontendConfig;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IUsuarioQuery _usuarioQuery;
        private readonly JwtTokenOptions _jwtTokenOptions;
        private readonly IRateLimitService _rateLimitService;
        private readonly RateLimitConfig _rateLimitConfig;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LabTechSaudeIdentityDbContext _identityDbContext;

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
            IHttpContextAccessor httpContextAccessor,
            IOptions<UrlsConfig> frontendConfig,
            LabTechSaudeIdentityDbContext identityDbContext)
            : base(notificador, unitOfWork, commandDispatcher)
        {
            _frontendConfig = frontendConfig.Value;
            _userManager = userManager;
            _signInManager = signInManager;
            _usuarioQuery = usuarioQuery;
            _jwtTokenOptions = jwtTokenOptions.Value;
            _rateLimitService = rateLimitService;
            _rateLimitConfig = rateLimitConfig.Value;
            _httpContextAccessor = httpContextAccessor;
            _identityDbContext = identityDbContext;
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

            return await GerarLoginResponse(user, usuario, cancellationToken);
        }

        private async Task<LoginResponse?> GerarLoginResponse(
            IdentityUser user,
            UsuarioViewModel usuario,
            CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;
            var notBefore = now.UtcDateTime;
            var expires = notBefore.AddHours(1);
            var familyId = Guid.NewGuid();
            var absoluteExpiresAt = now.AddDays(_jwtTokenOptions.RefreshTokenAbsoluteExpirationDays);
            var refreshToken = GerarRefreshToken();
            var refreshTokenEntity = CriarRefreshTokenEntity(user.Id, familyId, now, absoluteExpiresAt, refreshToken);
            var accessToken = await GerarAccessToken(user, notBefore, expires);

            _identityDbContext.RefreshTokens.Add(refreshTokenEntity);
            await _identityDbContext.SaveChangesAsync(cancellationToken);

            return CriarLoginResponse(usuario, accessToken, refreshToken, notBefore, expires);
        }

        private string GerarRefreshToken()
        {
            return WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(64));
        }

        public async Task<LoginResponse?> Handle(
            RenovarRefreshTokenCommand command,
            CancellationToken cancellationToken = default)
        {
            const string tokenInvalido = "Refresh token inválido ou expirado.";

            if (string.IsNullOrWhiteSpace(command.RefreshToken))
            {
                NotificarErro(tokenInvalido);
                return null;
            }

            var tokenHash = CalcularHash(command.RefreshToken);

            await using var transaction = await _identityDbContext.Database.BeginTransactionAsync(cancellationToken);
            var currentToken = await _identityDbContext.RefreshTokens
                .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

            if (currentToken == null)
            {
                NotificarErro(tokenInvalido);
                return null;
            }

            await BloquearFamilia(currentToken.FamilyId, cancellationToken);
            await _identityDbContext.Entry(currentToken).ReloadAsync(cancellationToken);
            var now = DateTimeOffset.UtcNow;

            if (currentToken.RevokedAt.HasValue)
            {
                await RevogarFamilia(currentToken.FamilyId, now, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                NotificarErro(tokenInvalido);
                return null;
            }

            if (currentToken.ExpiresAt <= now || currentToken.AbsoluteExpiresAt <= now)
            {
                NotificarErro(tokenInvalido);
                return null;
            }

            var user = await _userManager.FindByIdAsync(currentToken.UserId);
            if (user == null)
            {
                NotificarErro(tokenInvalido);
                return null;
            }

            var usuario = await _usuarioQuery.ObterPorId(Guid.Parse(user.Id));
            if (usuario == null)
            {
                NotificarErro(tokenInvalido);
                return null;
            }

            var nextTokenId = Guid.NewGuid();
            var refreshToken = GerarRefreshToken();

            var refreshTokenEntity = CriarRefreshTokenEntity(
                user.Id,
                currentToken.FamilyId,
                now,
                currentToken.AbsoluteExpiresAt,
                refreshToken,
                nextTokenId);

            var notBefore = now.UtcDateTime;
            var expires = notBefore.AddHours(1);
            var accessToken = await GerarAccessToken(user, notBefore, expires);

            _identityDbContext.RefreshTokens.Add(refreshTokenEntity);
            await _identityDbContext.SaveChangesAsync(cancellationToken);

            var updated = await _identityDbContext.RefreshTokens
                .Where(token =>
                    token.Id == currentToken.Id
                    && token.RevokedAt == null
                    && token.ExpiresAt > now
                    && token.AbsoluteExpiresAt > now)
                .ExecuteUpdateAsync(updates => updates
                    .SetProperty(token => token.RevokedAt, now)
                    .SetProperty(token => token.ReplacedByTokenId, nextTokenId), cancellationToken);

            if (updated == 0)
            {
                await RevogarFamilia(currentToken.FamilyId, now, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                NotificarErro(tokenInvalido);
                return null;
            }

            await transaction.CommitAsync(cancellationToken);

            return CriarLoginResponse(usuario, accessToken, refreshToken, notBefore, expires);
        }

        public async Task<bool> Handle(
            RevogarRefreshTokenCommand command,
            CancellationToken cancellationToken = default)
        {
            const string tokenInvalido = "Refresh token inválido.";

            if (string.IsNullOrWhiteSpace(command.RefreshToken))
                return NotificarErro(tokenInvalido);

            var tokenHash = CalcularHash(command.RefreshToken);
            await using var transaction = await _identityDbContext.Database.BeginTransactionAsync(cancellationToken);
            var token = await _identityDbContext.RefreshTokens
                .AsNoTracking()
                .SingleOrDefaultAsync(refreshToken => refreshToken.TokenHash == tokenHash, cancellationToken);

            if (token == null)
                return NotificarErro(tokenInvalido);

            await BloquearFamilia(token.FamilyId, cancellationToken);
            await RevogarFamilia(token.FamilyId, DateTimeOffset.UtcNow, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }

        private async Task BloquearFamilia(Guid familyId, CancellationToken cancellationToken)
        {
            await _identityDbContext.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_advisory_xact_lock(hashtextextended({familyId.ToString()}, 0))",
                cancellationToken);
        }

        private async Task RevogarFamilia(
            Guid familyId,
            DateTimeOffset revokedAt,
            CancellationToken cancellationToken)
        {
            await _identityDbContext.RefreshTokens
                .Where(token => token.FamilyId == familyId && token.RevokedAt == null)
                .ExecuteUpdateAsync(
                    updates => updates.SetProperty(token => token.RevokedAt, revokedAt),
                    cancellationToken);
        }

        private RefreshToken CriarRefreshTokenEntity(
            string userId,
            Guid familyId,
            DateTimeOffset now,
            DateTimeOffset absoluteExpiresAt,
            string token,
            Guid? tokenId = null)
        {
            var expiresAt = now.AddDays(_jwtTokenOptions.RefreshTokenSlidingExpirationDays);
            if (expiresAt > absoluteExpiresAt)
                expiresAt = absoluteExpiresAt;

            return new RefreshToken
            {
                Id = tokenId ?? Guid.NewGuid(),
                UserId = userId,
                FamilyId = familyId,
                TokenHash = CalcularHash(token),
                CreatedAt = now,
                ExpiresAt = expiresAt,
                AbsoluteExpiresAt = absoluteExpiresAt
            };
        }

        private static string CalcularHash(string token)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        }

        private static LoginResponse CriarLoginResponse(
            UsuarioViewModel usuario,
            string accessToken,
            string refreshToken,
            DateTime notBefore,
            DateTime expires)
        {
            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
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

            if (!await EnviarCodigoConfirmarEmail(user, cancellationToken))
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

            if (string.IsNullOrWhiteSpace(_frontendConfig.FrontendUrl))
                return NotificarErro("Não foi possível enviar o link: URL do frontend não configurada.");

            var user = await _userManager.FindByEmailAsync(command.Email);

            // Resposta idêntica para e-mails inexistentes ou não confirmados, evitando enumeração de usuários.
            if (user == null || string.IsNullOrWhiteSpace(user.Email) || !user.EmailConfirmed)
                return true;

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var codigo = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var link = $"{_frontendConfig.FrontendUrl.TrimEnd('/')}/redefinir-senha" +
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
