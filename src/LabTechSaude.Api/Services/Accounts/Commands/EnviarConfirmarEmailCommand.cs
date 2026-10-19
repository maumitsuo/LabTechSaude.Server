using LabTechSaude.Application.Commands.Messages;

namespace LabTechSaude.Api.Services.Accounts.Commands
{
    public class EnviarConfirmarEmailCommand : ICommand
    {
        public string Email { get; private set; }

        public EnviarConfirmarEmailCommand(string email)
        {
            Email = email;
        }
    }
}
