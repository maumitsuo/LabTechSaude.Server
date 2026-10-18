using System.ComponentModel.DataAnnotations;

namespace LabTechSaude.Api.ViewModels
{
    public class EnviarCodigoConfirmarEmailViewModel
    {
        [Required(ErrorMessage = "Favor preencher o e-mail.")]
        [EmailAddress(ErrorMessage = "Favor preencher um e-mail válido.")]
        public string Email { get; set; } = string.Empty;
    }
}
