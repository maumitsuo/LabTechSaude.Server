using System.Security.Cryptography;
using System.Text;
using LabTechSaude.Api.Configs.RateLimit;
using LabTechSaude.Api.Services.RateLimiting;
using LabTechSaude.Application.Notifications;
using Microsoft.Extensions.Options;

namespace LabTechSaude.Api.Services.Accounts.RateLimiting
{
    public class RateLimitAuthenticationService : IRateLimitAuthenticationService
    {
        private readonly Notificador _notificador;
        private readonly IRateLimitService _rateLimitService;
        private readonly RateLimitConfig _rateLimitConfig;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RateLimitAuthenticationService(
            Notificador notificador,
            IRateLimitService rateLimitService,
            IOptions<RateLimitConfig> rateLimitConfig,
            IHttpContextAccessor httpContextAccessor)
        {
            _notificador = notificador;
            _rateLimitService = rateLimitService;
            _rateLimitConfig = rateLimitConfig.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> PermitirEnvioCodigoAsync(
            string operacao,
            string email,
            string mensagemCooldown,
            string mensagemLimite)
        {
            var chave = ChaveEmail(operacao, email);

            var resultado = await _rateLimitService.TentarConsumirAsync(
                chave + ":cooldown", 1, TimeSpan.FromSeconds(_rateLimitConfig.EnvioCodigoCooldownSegundos), TimeSpan.Zero);
            var mensagem = mensagemCooldown;

            if (resultado.Permitido)
            {
                resultado = await _rateLimitService.TentarConsumirAsync(
                    chave,
                    _rateLimitConfig.EnvioCodigoMaximoPorHora,
                    TimeSpan.FromHours(1),
                    TimeSpan.FromMinutes(_rateLimitConfig.EnvioCodigoBloqueioMinutos));
                mensagem = mensagemLimite;
            }

            if (!resultado.Permitido)
                NotificarLimiteExcedido(mensagem, resultado.RetryAfter);

            return resultado.Permitido;
        }

        private static string ChaveEmail(string operacao, string email)
        {
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(email.Trim().ToLowerInvariant())));
            return $"{operacao}:{hash}";
        }

        private void NotificarLimiteExcedido(string mensagem, TimeSpan retryAfter)
        {
            var segundos = (int)Math.Ceiling(retryAfter.TotalSeconds);

            if (_httpContextAccessor.HttpContext is { } httpContext)
                httpContext.Items[RateLimitHttpItems.RetryAfterSegundos] = segundos;

            _notificador.IncluirNotificacao($"{mensagem} Tente novamente em {segundos} segundos.");
        }
    }
}
