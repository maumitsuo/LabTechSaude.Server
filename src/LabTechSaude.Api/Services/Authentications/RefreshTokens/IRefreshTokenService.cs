namespace LabTechSaude.Api.Services.Authentications.RefreshTokens
{
    public interface IRefreshTokenService
    {
        // Inicia uma nova sessão (família) e devolve o valor original do refresh token.
        Task<string> EmitirAsync(string userId, CancellationToken cancellationToken = default);

        // Identifica o usuário dono do refresh token. Se não existir, notifica o erro e retorna null.
        Task<string?> ObterUserIdAsync(string refreshToken, CancellationToken cancellationToken = default);

        // Consome o refresh token e o substitui por um novo; reutilização revoga a família.
        // Em caso de falha, notifica o erro e retorna null.
        Task<string?> RenovarAsync(string refreshToken, CancellationToken cancellationToken = default);

        // Revoga a família (sessão) do refresh token informado. Em caso de falha, notifica o erro e retorna false.
        Task<bool> RevogarAsync(string refreshToken, CancellationToken cancellationToken = default);

        Task RevogarTodosDoUsuarioAsync(string userId, CancellationToken cancellationToken = default);
    }
}
