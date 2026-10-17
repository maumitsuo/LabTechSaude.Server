using LabTechSaude.Application.Commands.Messages;

namespace LabTechSaude.Api.Services.Authentications.Commands
{
    public class ConfirmarCodigoEmailCommand : ICommand
    {
        public string Email { get; private set; }
        public string Code { get; private set; }

        public ConfirmarCodigoEmailCommand(string email, string code)
        {
            Email = email;
            Code = code;
        }
    }
}
