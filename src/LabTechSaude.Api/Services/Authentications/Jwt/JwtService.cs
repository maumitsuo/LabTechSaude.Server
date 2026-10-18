using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LabTechSaude.Api.Configs.Identity;
using LabTechSaude.Api.Models.Authentications;
using LabTechSaude.Api.Services.Authentications.RefreshTokens;
using LabTechSaude.Application.Notifications;
using LabTechSaude.Application.Services.Usuarios.Queries;
using LabTechSaude.Application.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LabTechSaude.Api.Services.Authentications.Jwt
{
    public class JwtService : IJwtService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUsuarioQuery _usuarioQuery;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly Notificador _notificador;
        private readonly JwtOptions _jwtOptions;

        public JwtService(
            Notificador notificador,
            UserManager<IdentityUser> userManager,
            IUsuarioQuery usuarioQuery,
            IRefreshTokenService refreshTokenService,
            IOptions<JwtOptions> jwtOptions)
        {
            _notificador = notificador;
            _userManager = userManager;
            _usuarioQuery = usuarioQuery;
            _refreshTokenService = refreshTokenService;
            _jwtOptions = jwtOptions.Value;
        }

        public async Task<LoginResponse> GerarLoginResponseAsync(
            IdentityUser user,
            UsuarioViewModel usuario,
            CancellationToken cancellationToken = default)
        {
            var refreshToken = await _refreshTokenService.EmitirAsync(user.Id, cancellationToken);

            return await CriarLoginResponseAsync(user, usuario, refreshToken);
        }

        public async Task<LoginResponse?> RenovarAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            LoginResponse? login = null;
            var userId = await _refreshTokenService.ObterUserIdAsync(refreshToken, cancellationToken);

            if (userId != null)
            {
                var user = await _userManager.FindByIdAsync(userId);
                var usuario = await _usuarioQuery.ObterPorId(Guid.Parse(userId));

                if (user == null || usuario == null)
                {
                    _notificador.IncluirNotificacao("Refresh token inválido ou expirado.");
                }
                else
                {
                    var novoRefreshToken = await _refreshTokenService.RenovarAsync(refreshToken, cancellationToken);

                    if (novoRefreshToken != null)
                        login = await CriarLoginResponseAsync(user, usuario, novoRefreshToken);
                }
            }

            return login;
        }

        public Task<bool> RevogarAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            return _refreshTokenService.RevogarAsync(refreshToken, cancellationToken);
        }

        public Task RevogarSessoesDoUsuarioAsync(string userId, CancellationToken cancellationToken = default)
        {
            return _refreshTokenService.RevogarTodosDoUsuarioAsync(userId, cancellationToken);
        }

        private async Task<LoginResponse> CriarLoginResponseAsync(
            IdentityUser user,
            UsuarioViewModel usuario,
            string refreshToken)
        {
            var notBefore = DateTime.UtcNow;
            var expires = notBefore.AddHours(1);
            var accessToken = await GerarAccessToken(user, notBefore, expires);

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

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtOptions.SecretKey));

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                notBefore: notBefore,
                expires: expires,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
