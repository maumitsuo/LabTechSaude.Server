using LabTechSaude.Api.Applications.Core;
using LabTechSaude.Api.Extensions;
using LabTechSaude.Api.Notifications;
using LabTechSaude.Api.ViewModels;
using LabTechSaude.Domain.Core.Data;
using LabTechSaude.Domain.Core.ValueObjects;
using LabTechSaude.Domain.Pessoas;

namespace LabTechSaude.Api.Applications.Pessoas
{
    public class PessoaService : BaseService, IPessoaService
    {
        private readonly IPessoaRepository _pessoaRepository;

        public PessoaService(
            Notificador notificador,
            IUnitOfWork unitOfWork,
            IPessoaRepository pessoaRepository)
            : base(notificador, unitOfWork)
        {
            _pessoaRepository = pessoaRepository;
        }

        public async Task<PessoaViewModel?> ObterPorId(Guid id)
        {
            return (await _pessoaRepository.ObterPorId(id))?.ToViewModel();
        }

        public async Task<IEnumerable<PessoaViewModel>> ObterTodos()
        {
            return (await _pessoaRepository.ObterTodos()).ToViewModel();
        }

        private async Task PessoaExiste(Guid id)
        {
            if (!await _pessoaRepository.ExisteId(id))
                NotificarErro(PessoaValidation.NotFound_Message);
        }

        private async Task<bool> PessoaExiste(Pessoa? pessoa)
        {
            if (pessoa is null)
                return NotificarErro(PessoaValidation.NotFound_Message);

            return true;
        }

        private async Task PessoaEhUnica(Pessoa pessoa)
        {
            if (!await _pessoaRepository.EhUnico(pessoa))
                NotificarErro(PessoaValidation.Unique_Message);
        }

        private async Task<bool> PessoaAptaParaCadastrar(Pessoa pessoa)
        {
            await ValidarEntidade(pessoa);
            await PessoaEhUnica(pessoa);

            return CommandEhValido();
        }

        public async Task<bool> Cadastrar(PessoaViewModel pessoaViewModel)
        {
            var pessoa = new Pessoa(Guid.NewGuid(), pessoaViewModel.Nome, new Cpf(pessoaViewModel.Cpf));

            if (!await PessoaAptaParaCadastrar(pessoa))
                return false;

            await _pessoaRepository.Cadastrar(pessoa);

            await Commit();

            return CommandEhValido();
        }

        private async Task<bool> PessoaAptaParaAtualizar(Pessoa? pessoa)
        {
            if (!await PessoaExiste(pessoa))
                return false;

            await ValidarEntidade(pessoa!);
            await PessoaEhUnica(pessoa!);

            return CommandEhValido();
        }

        public async Task<bool> Atualizar(PessoaViewModel pessoaViewModel)
        {
            var pessoa = await _pessoaRepository.ObterPorId(pessoaViewModel.Id);
            
            pessoa?.AlterarNome(pessoaViewModel.Nome);

            if (!await PessoaAptaParaAtualizar(pessoa))
                return false;

            _pessoaRepository.Atualizar(pessoa!);

            await Commit();

            return CommandEhValido();
        }

        private async Task<bool> PessoaAptaParaExcluir(Guid id)
        {
            await PessoaExiste(id);

            return CommandEhValido();
        }

        public async Task<bool> Excluir(Guid id)
        {
            if (!await PessoaAptaParaExcluir(id))
                return false;

            await _pessoaRepository.Excluir(id);

            await Commit();

            return CommandEhValido();
        }
    }
}
