using LabTechSaude.Domain.Core.ValueObjects;

namespace LabTechSaude.Api.ViewModels
{
    public record PessoaViewModel(Guid Id, string Nome, string Cpf);
}
