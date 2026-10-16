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
        public string SenhaConfirmacao { get; private set; } = string.Empty;
    }
}
