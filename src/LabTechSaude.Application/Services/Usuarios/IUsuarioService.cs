using LabTechSaude.Application.ViewModels;

namespace LabTechSaude.Application.Services.Usuarios
{
    public interface IUsuarioService
    {
        Task<UsuarioViewModel?> ObterPorId(Guid id);
        Task<IEnumerable<UsuarioViewModel>> ObterTodos();

        Task<bool> Cadastrar(UsuarioViewModel usuarioViewModel);
        Task<bool> Atualizar(UsuarioViewModel usuarioViewModel);
        Task<bool> Excluir(Guid id);
    }
}
