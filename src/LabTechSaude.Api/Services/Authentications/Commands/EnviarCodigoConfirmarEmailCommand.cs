using LabTechSaude.Application.Commands.Messages;

namespace LabTechSaude.Api.Services.Authentications.Commands
{
    public class EnviarCodigoConfirmarEmailCommand : ICommand
    {
        public Guid UsuarioId { get; private set; }

        public EnviarCodigoConfirmarEmailCommand(Guid usuarioId)
        {
            UsuarioId = usuarioId;
        }
    }
}
