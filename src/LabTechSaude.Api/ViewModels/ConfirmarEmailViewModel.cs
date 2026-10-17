using System.ComponentModel.DataAnnotations;

namespace LabTechSaude.Api.ViewModels
{
    public class ConfirmarEmailViewModel
    {
        [Required]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [RegularExpression("^[0-9]{6}$", ErrorMessage = "O código de confirmação deve conter 6 dígitos.")]
        public string Code { get; set; } = string.Empty;
    }
}
