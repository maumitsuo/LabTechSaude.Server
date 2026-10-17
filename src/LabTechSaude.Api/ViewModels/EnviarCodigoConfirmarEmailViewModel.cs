using System.ComponentModel.DataAnnotations;

namespace LabTechSaude.Api.ViewModels
{
    public class EnviarCodigoConfirmarEmailViewModel
    {
        [Required(ErrorMessage = "Favor preencher o usuário.")]
        public Guid UsuarioId { get; private set; }
    }
}
