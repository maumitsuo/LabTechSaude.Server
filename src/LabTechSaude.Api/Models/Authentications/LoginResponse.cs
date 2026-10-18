using LabTechSaude.Application.ViewModels;

namespace LabTechSaude.Api.Models.Authentications
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime NotBefore { get; set; }
        public DateTime Expires { get; set; }
        public UsuarioResponse Usuario { get; set; } = null!;
    }
}
