using LabTechSaude.Application.Extensions;
using LabTechSaude.Application.Notifications;
using LabTechSaude.Application.ViewModels;
using LabTechSaude.Domain.Core.Data;
using LabTechSaude.Domain.Core.ValueObjects;
using LabTechSaude.Domain.Usuarios;

namespace LabTechSaude.Application.Services.Usuarios
{
    public class UsuarioService : BaseService, IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public UsuarioService(
            Notificador notificador,
            IUnitOfWork unitOfWork,
            IUsuarioRepository usuarioRepository)
            : base(notificador, unitOfWork)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<UsuarioViewModel?> ObterPorId(Guid id)
        {
            return (await _usuarioRepository.ObterPorId(id))?.ToViewModel();
        }

        public async Task<IEnumerable<UsuarioViewModel>> ObterTodos()
        {
            return (await _usuarioRepository.ObterTodos()).ToViewModel();
        }

        private async Task UsuarioExiste(Guid id)
        {
            if (!await _usuarioRepository.ExisteId(id))
                NotificarErro(UsuarioValidation.NotFound_Message);
        }

        private async Task<bool> UsuarioExiste(Usuario? usuario)
        {
            if (usuario is null)
                return NotificarErro(UsuarioValidation.NotFound_Message);

            return true;
        }

        private async Task UsuarioEhUnico(Usuario usuario)
        {
            if (!await _usuarioRepository.EhUnico(usuario))
                NotificarErro(UsuarioValidation.Unique_Message);
        }

        private async Task<bool> UsuarioAptoParaCadastrar(Usuario usuario)
        {
            await ValidarEntidade(usuario);
            await UsuarioEhUnico(usuario);

            return CommandEhValido();
        }

        public async Task<bool> Cadastrar(UsuarioViewModel usuarioViewModel)
        {
            var usuario = new Usuario(Guid.NewGuid(), usuarioViewModel.Nome, new Cpf(usuarioViewModel.Cpf));

            if (!await UsuarioAptoParaCadastrar(usuario))
                return false;

            await _usuarioRepository.Cadastrar(usuario);

            await Commit();

            return CommandEhValido();
        }

        private async Task<bool> UsuarioAptoParaAtualizar(Usuario? usuario)
        {
            if (!await UsuarioExiste(usuario))
                return false;

            await ValidarEntidade(usuario!);
            await UsuarioEhUnico(usuario!);

            return CommandEhValido();
        }

        public async Task<bool> Atualizar(UsuarioViewModel usuarioViewModel)
        {
            var usuario = await _usuarioRepository.ObterPorId(usuarioViewModel.Id);
            
            usuario?.AlterarNome(usuarioViewModel.Nome);

            if (!await UsuarioAptoParaAtualizar(usuario))
                return false;

            _usuarioRepository.Atualizar(usuario!);

            await Commit();

            return CommandEhValido();
        }

        private async Task<bool> UsuarioAptoParaExcluir(Guid id)
        {
            await UsuarioExiste(id);

            return CommandEhValido();
        }

        public async Task<bool> Excluir(Guid id)
        {
            if (!await UsuarioAptoParaExcluir(id))
                return false;

            await _usuarioRepository.Excluir(id);

            await Commit();

            return CommandEhValido();
        }
    }
}
