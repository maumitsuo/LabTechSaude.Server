using LabTechSaude.Api.ViewModels;

namespace LabTechSaude.Api.Applications.Pessoas
{
    public interface IPessoaService
    {
        Task<PessoaViewModel?> ObterPorId(Guid id);
        Task<IEnumerable<PessoaViewModel>> ObterTodos();

        Task<bool> Cadastrar(PessoaViewModel pessoaViewModel);
        Task<bool> Atualizar(PessoaViewModel pessoaViewModel);
        Task<bool> Excluir(Guid id);
    }
}
