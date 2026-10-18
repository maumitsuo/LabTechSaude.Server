using LabTechSaude.Api.Models.Authentications;
using LabTechSaude.Application.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace LabTechSaude.Api.Services.Authentications.Jwt
{
    public interface IJwtService
    {
        // Emite o access token e uma nova sessão de refresh token após autenticação bem-sucedida.
        Task<LoginResponse> GerarLoginResponseAsync(IdentityUser user, UsuarioViewModel usuario, CancellationToken cancellationToken = default);

        // Em caso de falha, notifica o erro e retorna null.
        Task<LoginResponse?> RenovarAsync(string refreshToken, CancellationToken cancellationToken = default);

        // Em caso de falha, notifica o erro e retorna false.
        Task<bool> RevogarAsync(string refreshToken, CancellationToken cancellationToken = default);

        Task RevogarSessoesDoUsuarioAsync(string userId, CancellationToken cancellationToken = default);
    }
}
