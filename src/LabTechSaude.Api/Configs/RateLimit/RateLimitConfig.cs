namespace LabTechSaude.Api.Configs.RateLimit
{
    public class RateLimitConfig
    {
        public int IpRequisicoesPorMinuto { get; set; } = 5;

        public int EnvioCodigoCooldownSegundos { get; set; } = 60;
        public int EnvioCodigoMaximoPorHora { get; set; } = 5;
        public int EnvioCodigoBloqueioMinutos { get; set; } = 60;

        public int ConfirmacaoCodigoMaximoFalhas { get; set; } = 5;
        public int ConfirmacaoCodigoJanelaMinutos { get; set; } = 15;
        public int ConfirmacaoCodigoBloqueioMinutos { get; set; } = 15;
    }
}
