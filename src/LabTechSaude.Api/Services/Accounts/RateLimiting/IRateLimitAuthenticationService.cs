namespace LabTechSaude.Api.Services.Accounts.RateLimiting
{
    public interface IRateLimitAuthenticationService
    {
        // Aplica cooldown e limite por hora ao envio de códigos/links para o e-mail.
        // Se excedido, notifica o erro (com Retry-After) e retorna false.
        Task<bool> PermitirEnvioCodigoAsync(
            string operacao,
            string email,
            string mensagemCooldown,
            string mensagemLimite);
    }
}
