using LabTechSaude.Application.Commands.Messages;

namespace LabTechSaude.Api.Services.Accounts.Commands
{
    public class RedefinirSenhaCommand : ICommand
    {
        public string Email { get; private set; }
        public string Codigo { get; private set; }
        public string Senha { get; private set; }

        public RedefinirSenhaCommand(string email, string codigo, string senha)
        {
            Email = email;
            Codigo = codigo;
            Senha = senha;
        }
    }
}
