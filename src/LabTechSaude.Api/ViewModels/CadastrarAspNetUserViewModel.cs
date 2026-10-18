using LabTechSaude.Domain.Core.ValueObjects;
using LabTechSaude.Domain.Usuarios;
using System.ComponentModel.DataAnnotations;

namespace LabTechSaude.Api.ViewModels
{
    public class CadastrarAspNetUserViewModel
    {
        [Required(ErrorMessage = UsuarioValidation.Nome_Required_Message)]
        [StringLength(
            UsuarioValidation.Nome_MaxLength,
            ErrorMessage = UsuarioValidation.Nome_Length_Message,
            MinimumLength = UsuarioValidation.Nome_MinLength)]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = EmailValidation.Email_Required_Message)]
        [MaxLength(EmailValidation.Email_MaxLength, ErrorMessage = EmailValidation.Email_MaxLength_Message)]
        [EmailAddress(ErrorMessage = EmailValidation.Email_Invalid_Message)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = CpfValidation.Cpf_Required_Message)]
        [StringLength(
            CpfValidation.Cpf_Length,
            ErrorMessage = CpfValidation.Cpf_Length_Message,
            MinimumLength = CpfValidation.Cpf_Length)]
        public string Cpf { get; set; } = string.Empty;

        [Required(ErrorMessage = "Favor preencher a senha.")]
        [MinLength(6, ErrorMessage = "A senha deve ter no mínimo {1} caracteres.")]
        [DataType(DataType.Password)]
        public string Senha { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Senha", ErrorMessage = "A confirmação da senha não confere.")]
        public string SenhaConfirmacao { get; set; } = string.Empty;
    }
}
