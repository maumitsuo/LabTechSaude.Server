using LabTechSaude.Application.Commands.Messages;

namespace LabTechSaude.Api.Services.Authentications.Commands
{
    public class CadastrarAspNetUserCommand : Command
    {
        public Guid Id { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string Cpf { get; private set; } = string.Empty;
        public string Senha { get; private set; } = string.Empty;
        
        public CadastrarAspNetUserCommand(string nome, string email, string cpf, string senha)
        {
            Id = Guid.NewGuid();
            Nome = nome;
            Email = email;
            Cpf = cpf;
            Senha = senha;
        }
    }
}
