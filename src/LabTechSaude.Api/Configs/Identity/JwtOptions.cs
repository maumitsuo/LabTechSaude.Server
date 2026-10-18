namespace LabTechSaude.Api.Configs.Identity
{
    public class JwtOptions
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public int Expiration { get; set; }
        public int RefreshTokenSlidingExpirationDays { get; set; } = 1;
        public int RefreshTokenAbsoluteExpirationDays { get; set; } = 7;
    }
}
