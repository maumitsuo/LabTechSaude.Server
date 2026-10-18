namespace LabTechSaude.Api.Services.RateLimiting
{
    public readonly record struct RateLimitResult(bool Permitido, TimeSpan RetryAfter)
    {
        public static RateLimitResult Liberado => new(true, TimeSpan.Zero);
    }

    public interface IRateLimitService
    {
        /// <summary>
        /// Consome uma tentativa. Ao exceder o limite na janela, bloqueia a chave por
        /// <paramref name="bloqueio"/> (ou, se zero, até o fim da janela).
        /// </summary>
        Task<RateLimitResult> TentarConsumirAsync(string chave, int limite, TimeSpan janela, TimeSpan bloqueio);

        /// <summary>Consulta se a chave está bloqueada, sem consumir tentativa.</summary>
        Task<RateLimitResult> ConsultarBloqueioAsync(string chave);

        /// <summary>Zera o contador da chave.</summary>
        Task ResetarAsync(string chave);
    }
}
