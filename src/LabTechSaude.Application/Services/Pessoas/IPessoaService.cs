using LabTechSaude.Application.ViewModels;

namespace LabTechSaude.Application.Services.Pessoas
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
