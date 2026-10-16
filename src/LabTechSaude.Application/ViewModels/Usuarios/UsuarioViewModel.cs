using FluentValidation.Validators;
using LabTechSaude.Domain.Core.ValueObjects;
using LabTechSaude.Domain.Usuarios;
using System.ComponentModel.DataAnnotations;

namespace LabTechSaude.Application.ViewModels.Usuarios
{
    public class UsuarioViewModel 
    {
        [Key]
        public Guid Id { get; set; }

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
    } 
}
