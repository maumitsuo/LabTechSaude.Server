using Microsoft.AspNetCore.Identity;

namespace LabTechSaude.Api.Services.Authentications.Emails
{
    public interface IEmailAuthenticationService
    {
        // Gera o código de confirmação e envia o e-mail. Em caso de falha, notifica o erro e retorna false.
        Task<bool> EnviarConfirmarEmailAsync(IdentityUser user, CancellationToken cancellationToken = default);

        // Gera o link de redefinição e envia o e-mail. Em caso de falha, notifica o erro e retorna false.
        Task<bool> EnviarRedefinirSenhaAsync(IdentityUser user, CancellationToken cancellationToken = default);
    }
}
