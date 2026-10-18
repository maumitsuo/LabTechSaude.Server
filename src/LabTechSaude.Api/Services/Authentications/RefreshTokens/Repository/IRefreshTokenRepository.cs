using LabTechSaude.Api.Models.Authentications;

namespace LabTechSaude.Api.Services.Authentications.RefreshTokens.Repository
{
    public interface IRefreshTokenTransaction : IAsyncDisposable
    {
        Task CommitAsync(CancellationToken cancellationToken = default);
    }

    // Persistência dos refresh tokens no contexto de Identity, que é independente do IUnitOfWork do domínio.
    public interface IRefreshTokenRepository
    {
        Task<IRefreshTokenTransaction> IniciarTransacaoAsync(CancellationToken cancellationToken = default);
        Task<RefreshToken?> ObterPorHashAsync(string tokenHash, CancellationToken cancellationToken = default);
        Task RecarregarAsync(RefreshToken token, CancellationToken cancellationToken = default);
        Task BloquearFamiliaAsync(Guid familyId, CancellationToken cancellationToken = default);
        void Adicionar(RefreshToken token);
        Task SalvarAsync(CancellationToken cancellationToken = default);
        Task<bool> RevogarAtivoAsync(Guid tokenId, Guid substitutoId, DateTimeOffset now, CancellationToken cancellationToken = default);
        Task RevogarFamiliaAsync(Guid familyId, DateTimeOffset revokedAt, CancellationToken cancellationToken = default);
        Task RemoverExpiradosAsync(string userId, DateTimeOffset now, CancellationToken cancellationToken = default);
        Task RemoverPorUsuarioAsync(string userId, CancellationToken cancellationToken = default);
    }
}
