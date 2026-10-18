namespace LabTechSaude.Api.Models.Authentications
{
    public class UsuarioResponse
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public ICollection<ClaimResponse> Claims { get; set; }
    }
}
