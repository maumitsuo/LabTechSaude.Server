using LabTechSaude.Api.Context;
using LabTechSaude.Api.Models.Authentications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace LabTechSaude.Api.Services.Authentications.RefreshTokens.Repository
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly LabTechSaudeIdentityDbContext _context;

        public RefreshTokenRepository(LabTechSaudeIdentityDbContext context)
        {
            _context = context;
        }

        public async Task<IRefreshTokenTransaction> IniciarTransacaoAsync(CancellationToken cancellationToken = default)
        {
            return new RefreshTokenTransaction(await _context.Database.BeginTransactionAsync(cancellationToken));
        }

        public Task<RefreshToken?> ObterPorHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        {
            return _context.RefreshTokens.SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);
        }

        public Task RecarregarAsync(RefreshToken token, CancellationToken cancellationToken = default)
        {
            return _context.Entry(token).ReloadAsync(cancellationToken);
        }

        public async Task BloquearFamiliaAsync(Guid familyId, CancellationToken cancellationToken = default)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_advisory_xact_lock(hashtextextended({familyId.ToString()}, 0))",
                cancellationToken);
        }

        public void Adicionar(RefreshToken token)
        {
            _context.RefreshTokens.Add(token);
        }

        public Task SalvarAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> RevogarAtivoAsync(
            Guid tokenId,
            Guid substitutoId,
            DateTimeOffset now,
            CancellationToken cancellationToken = default)
        {
            var updated = await _context.RefreshTokens
                .Where(token =>
                    token.Id == tokenId
                    && token.RevokedAt == null
                    && token.ExpiresAt > now
                    && token.AbsoluteExpiresAt > now)
                .ExecuteUpdateAsync(updates => updates
                    .SetProperty(token => token.RevokedAt, now)
                    .SetProperty(token => token.ReplacedByTokenId, substitutoId), cancellationToken);

            return updated > 0;
        }

        public Task RevogarFamiliaAsync(Guid familyId, DateTimeOffset revokedAt, CancellationToken cancellationToken = default)
        {
            return _context.RefreshTokens
                .Where(token => token.FamilyId == familyId && token.RevokedAt == null)
                .ExecuteUpdateAsync(
                    updates => updates.SetProperty(token => token.RevokedAt, revokedAt),
                    cancellationToken);
        }

        public Task RemoverExpiradosAsync(string userId, DateTimeOffset now, CancellationToken cancellationToken = default)
        {
            return _context.RefreshTokens
                .Where(token => token.UserId == userId && token.ExpiresAt <= now)
                .ExecuteDeleteAsync(cancellationToken);
        }

        public Task RemoverPorUsuarioAsync(string userId, CancellationToken cancellationToken = default)
        {
            return _context.RefreshTokens
                .Where(token => token.UserId == userId)
                .ExecuteDeleteAsync(cancellationToken);
        }

        private sealed class RefreshTokenTransaction : IRefreshTokenTransaction
        {
            private readonly IDbContextTransaction _transaction;

            public RefreshTokenTransaction(IDbContextTransaction transaction)
            {
                _transaction = transaction;
            }

            public Task CommitAsync(CancellationToken cancellationToken = default) =>
                _transaction.CommitAsync(cancellationToken);

            public ValueTask DisposeAsync() => _transaction.DisposeAsync();
        }
    }
}
