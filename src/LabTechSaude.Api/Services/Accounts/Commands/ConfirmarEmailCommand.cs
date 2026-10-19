using LabTechSaude.Application.Commands.Messages;

namespace LabTechSaude.Api.Services.Accounts.Commands
{
    public class ConfirmarEmailCommand : ICommand
    {
        public string Email { get; private set; }
        public string Code { get; private set; }

        public ConfirmarEmailCommand(string email, string code)
        {
            Email = email;
            Code = code;
        }
    }
}
