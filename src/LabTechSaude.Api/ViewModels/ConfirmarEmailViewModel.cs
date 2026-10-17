using System.ComponentModel.DataAnnotations;

namespace LabTechSaude.Api.ViewModels
{
    public class ConfirmarEmailViewModel
    {
        [Required]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "O código de confirmação inválido.")]
        public string Code { get; set; } = string.Empty;
    }
}
