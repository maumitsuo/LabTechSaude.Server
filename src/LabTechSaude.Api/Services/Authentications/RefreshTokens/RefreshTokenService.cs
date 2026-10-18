using System.Security.Cryptography;
using System.Text;
using LabTechSaude.Api.Configs.Identity;
using LabTechSaude.Api.Models.Authentications;
using LabTechSaude.Api.Services.Authentications.RefreshTokens.Repository;
using LabTechSaude.Application.Notifications;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace LabTechSaude.Api.Services.Authentications.RefreshTokens
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private const string TokenInvalidoOuExpirado = "Refresh token inválido ou expirado.";
        private const string TokenInvalido = "Refresh token inválido.";

        private readonly IRefreshTokenRepository _repository;
        private readonly Notificador _notificador;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtOptions _jwtOptions;

        public RefreshTokenService(
            Notificador notificador,
            IRefreshTokenRepository repository,
            UserManager<IdentityUser> userManager,
            IOptions<JwtOptions> jwtOptions)
        {
            _notificador = notificador;
            _repository = repository;
            _userManager = userManager;
            _jwtOptions = jwtOptions.Value;
        }

        public async Task<string> EmitirAsync(string userId, CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;
            var absoluteExpiresAt = now.AddDays(_jwtOptions.RefreshTokenAbsoluteExpirationDays);
            var refreshToken = GerarRefreshToken();
            var entity = CriarEntidade(userId, Guid.NewGuid(), now, absoluteExpiresAt, refreshToken);

            await using var transaction = await _repository.IniciarTransacaoAsync(cancellationToken);
            await _repository.RemoverExpiradosAsync(userId, now, cancellationToken);
            _repository.Adicionar(entity);
            await _repository.SalvarAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return refreshToken;
        }

        public async Task<string?> ObterUserIdAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            string? userId = null;

            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                var token = await _repository.ObterPorHashAsync(CalcularHash(refreshToken), cancellationToken);
                userId = token?.UserId;
            }

            if (userId == null)
                _notificador.IncluirNotificacao(TokenInvalidoOuExpirado);

            return userId;
        }

        public async Task<string?> RenovarAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            string? novoRefreshToken = null;

            if (string.IsNullOrWhiteSpace(refreshToken))
                _notificador.IncluirNotificacao(TokenInvalidoOuExpirado);
            else
                novoRefreshToken = await RotacionarAsync(CalcularHash(refreshToken), cancellationToken);

            return novoRefreshToken;
        }

        public async Task<bool> RevogarAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            var revogado = false;

            if (string.IsNullOrWhiteSpace(refreshToken))
                _notificador.IncluirNotificacao(TokenInvalido);
            else
                revogado = await RevogarSessaoAsync(CalcularHash(refreshToken), cancellationToken);

            return revogado;
        }

        public Task RevogarTodosDoUsuarioAsync(string userId, CancellationToken cancellationToken = default)
        {
            return _repository.RemoverPorUsuarioAsync(userId, cancellationToken);
        }

        private async Task<string?> RotacionarAsync(string tokenHash, CancellationToken cancellationToken)
        {
            string? novoRefreshToken = null;

            await using var transaction = await _repository.IniciarTransacaoAsync(cancellationToken);
            var currentToken = await _repository.ObterPorHashAsync(tokenHash, cancellationToken);

            if (currentToken == null)
            {
                _notificador.IncluirNotificacao(TokenInvalidoOuExpirado);
            }
            else
            {
                await _repository.BloquearFamiliaAsync(currentToken.FamilyId, cancellationToken);
                await _repository.RecarregarAsync(currentToken, cancellationToken);
                var now = DateTimeOffset.UtcNow;

                if (currentToken.RevokedAt.HasValue)
                {
                    await RevogarFamiliaENotificarAsync(currentToken.FamilyId, now, transaction, cancellationToken);
                }
                else if (currentToken.ExpiresAt <= now || currentToken.AbsoluteExpiresAt <= now)
                {
                    _notificador.IncluirNotificacao(TokenInvalidoOuExpirado);
                }
                else
                {
                    var user = await _userManager.FindByIdAsync(currentToken.UserId);

                    if (user == null || !user.EmailConfirmed || await _userManager.IsLockedOutAsync(user))
                    {
                        await RevogarFamiliaENotificarAsync(currentToken.FamilyId, now, transaction, cancellationToken);
                    }
                    else
                    {
                        var nextTokenId = Guid.NewGuid();
                        var proximoRefreshToken = GerarRefreshToken();
                        var entity = CriarEntidade(user.Id, currentToken.FamilyId, now, currentToken.AbsoluteExpiresAt, proximoRefreshToken, nextTokenId);

                        _repository.Adicionar(entity);
                        await _repository.SalvarAsync(cancellationToken);

                        if (await _repository.RevogarAtivoAsync(currentToken.Id, nextTokenId, now, cancellationToken))
                        {
                            await transaction.CommitAsync(cancellationToken);
                            novoRefreshToken = proximoRefreshToken;
                        }
                        else
                        {
                            await RevogarFamiliaENotificarAsync(currentToken.FamilyId, now, transaction, cancellationToken);
                        }
                    }
                }
            }

            return novoRefreshToken;
        }

        private async Task<bool> RevogarSessaoAsync(string tokenHash, CancellationToken cancellationToken)
        {
            var revogado = false;

            await using var transaction = await _repository.IniciarTransacaoAsync(cancellationToken);
            var token = await _repository.ObterPorHashAsync(tokenHash, cancellationToken);

            if (token == null)
            {
                _notificador.IncluirNotificacao(TokenInvalido);
            }
            else
            {
                await _repository.BloquearFamiliaAsync(token.FamilyId, cancellationToken);
                await _repository.RevogarFamiliaAsync(token.FamilyId, DateTimeOffset.UtcNow, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                revogado = true;
            }

            return revogado;
        }

        private async Task RevogarFamiliaENotificarAsync(
            Guid familyId,
            DateTimeOffset now,
            IRefreshTokenTransaction transaction,
            CancellationToken cancellationToken)
        {
            await _repository.RevogarFamiliaAsync(familyId, now, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            _notificador.IncluirNotificacao(TokenInvalidoOuExpirado);
        }

        private static string GerarRefreshToken()
        {
            return WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(64));
        }

        private static string CalcularHash(string refreshToken)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
        }

        private RefreshToken CriarEntidade(
            string userId,
            Guid familyId,
            DateTimeOffset now,
            DateTimeOffset absoluteExpiresAt,
            string token,
            Guid? tokenId = null)
        {
            var expiresAt = now.AddDays(_jwtOptions.RefreshTokenSlidingExpirationDays);
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
    }
}
