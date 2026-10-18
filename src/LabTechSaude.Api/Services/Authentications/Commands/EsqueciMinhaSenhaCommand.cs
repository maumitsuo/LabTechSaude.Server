using LabTechSaude.Application.Commands.Messages;

namespace LabTechSaude.Api.Services.Authentications.Commands
{
    public class EsqueciMinhaSenhaCommand : ICommand
    {
        public string Email { get; private set; }

        public EsqueciMinhaSenhaCommand(string email)
        {
            Email = email;
        }
    }
}
