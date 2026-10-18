using StackExchange.Redis;

namespace LabTechSaude.Api.Services.RateLimiting
{
    // Se o Redis estiver indisponível, as requisições são liberadas (fail-open) e o erro é registrado.
    public class RedisRateLimitService : IRateLimitService
    {
        private const string Script = @"
local ttl = redis.call('PTTL', KEYS[2])
if ttl > 0 then return {0, ttl} end
local c = redis.call('INCR', KEYS[1])
if c == 1 then redis.call('PEXPIRE', KEYS[1], ARGV[2]) end
if c > tonumber(ARGV[1]) then
  local block = tonumber(ARGV[3])
  if block > 0 then
    redis.call('SET', KEYS[2], '1', 'PX', block)
    redis.call('DEL', KEYS[1])
    return {0, block}
  end
  return {0, redis.call('PTTL', KEYS[1])}
end
return {1, 0}";

        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<RedisRateLimitService> _logger;

        public RedisRateLimitService(IConnectionMultiplexer redis, ILogger<RedisRateLimitService> logger)
        {
            _redis = redis;
            _logger = logger;
        }

        public async Task<RateLimitResult> TentarConsumirAsync(string chave, int limite, TimeSpan janela, TimeSpan bloqueio)
        {
            try
            {
                var result = (long[]?)await _redis.GetDatabase().ScriptEvaluateAsync(
                    Script,
                    [Contador(chave), Bloqueio(chave)],
                    [limite, (long)janela.TotalMilliseconds, (long)bloqueio.TotalMilliseconds]);

                return result is { Length: 2 } && result[0] == 0
                    ? new RateLimitResult(false, TimeSpan.FromMilliseconds(Math.Max(result[1], 1000)))
                    : RateLimitResult.Liberado;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis indisponível: rate limit ignorado.");
                return RateLimitResult.Liberado;
            }
        }

        public async Task<RateLimitResult> ConsultarBloqueioAsync(string chave)
        {
            try
            {
                var ttl = await _redis.GetDatabase().KeyTimeToLiveAsync(Bloqueio(chave));

                return ttl is { } t && t > TimeSpan.Zero
                    ? new RateLimitResult(false, t)
                    : RateLimitResult.Liberado;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis indisponível: consulta de bloqueio ignorada.");
                return RateLimitResult.Liberado;
            }
        }

        public async Task ResetarAsync(string chave)
        {
            try
            {
                await _redis.GetDatabase().KeyDeleteAsync(Contador(chave));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis indisponível: reset de rate limit ignorado.");
            }
        }

        // As chaves {} mantêm contador e bloqueio no mesmo slot (Redis Cluster).
        private static RedisKey Contador(string chave) => $"LabTechSaude:rl:{{{chave}}}:cnt";
        private static RedisKey Bloqueio(string chave) => $"LabTechSaude:rl:{{{chave}}}:blk";
    }
}
