using LabTechSaude.Application.Commands.Messages;

namespace LabTechSaude.Api.Services.Authentications.Commands
{
    public class EnviarCodigoConfirmarEmailCommand : ICommand
    {
        public string Email { get; private set; }

        public EnviarCodigoConfirmarEmailCommand(string email)
        {
            Email = email;
        }
    }
}
