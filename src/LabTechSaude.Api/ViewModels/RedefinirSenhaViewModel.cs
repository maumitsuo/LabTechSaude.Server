using System.ComponentModel.DataAnnotations;

namespace LabTechSaude.Api.ViewModels
{
    public class RedefinirSenhaViewModel
    {
        [Required(ErrorMessage = "Favor preencher o e-mail.")]
        [EmailAddress(ErrorMessage = "Favor preencher um e-mail válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Favor preencher o código de redefinição.")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Favor preencher a senha.")]
        [MinLength(6, ErrorMessage = "A senha deve ter no mínimo {1} caracteres.")]
        [DataType(DataType.Password)]
        public string Senha { get; set; } = string.Empty;

        [Required(ErrorMessage = "Favor preencher a confirmação da senha.")]
        [DataType(DataType.Password)]
        [Compare("Senha", ErrorMessage = "Senha não confere.")]
        public string SenhaConfirmacao { get; set; } = string.Empty;
    }
}
