using LabTechSaude.Application.Commands.Messages;

namespace LabTechSaude.Api.Services.Authentications.Commands
{
    public class EfetuarLoginCommand : ICommand
    {
        public string Email { get; private set; }
        public string Senha { get; private set; }

        public EfetuarLoginCommand(string email, string senha)
        {
            Email = email;
            Senha = senha;
        }
    }
}
