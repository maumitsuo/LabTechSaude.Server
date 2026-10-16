namespace LabTechSaude.Api.Configs.Identity
{
    public class JwtTokenOptions
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public int Expiration { get; set; }
        public int RefreshTokenExpiration { get; set; }
    }
}
