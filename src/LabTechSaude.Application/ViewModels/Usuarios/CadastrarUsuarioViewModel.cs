using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LabTechSaude.Application.ViewModels.Usuarios
{
    public class CadastrarUsuarioViewModel : UsuarioViewModel
    {
        [Required(ErrorMessage = "Favor preencher a senha.")]
        [MinLength(6, ErrorMessage = "A senha deve ter no mínimo {1} caracteres.")]
        [DataType(DataType.Password)]
        public string Senha { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Senha", ErrorMessage = "A confirmação da senha não confere.")]
        public string SenhaConfirmacao { get; set; } = string.Empty;
    }
}
