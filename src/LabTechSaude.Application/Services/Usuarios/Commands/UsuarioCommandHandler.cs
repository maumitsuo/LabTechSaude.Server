using LabTechSaude.Application.Commands;
using LabTechSaude.Application.Commands.Dispatchers;
using LabTechSaude.Application.Commands.Handlers;
using LabTechSaude.Application.Notifications;
using LabTechSaude.Application.ViewModels;
using LabTechSaude.Domain.Core.Data;
using LabTechSaude.Domain.Core.ValueObjects;
using LabTechSaude.Domain.Usuarios;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Application.Services.Usuarios.Commands
{
    public class UsuarioCommandHandler : BaseCommandHandler,
        ICommandHandler<CadastrarUsuarioCommand>,
        ICommandHandler<AtualizarUsuarioCommand>,
        ICommandHandler<ExcluirUsuarioCommand>
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public UsuarioCommandHandler(
            Notificador notificador, 
            IUnitOfWork unitOfWork, 
            ICommandDispatcher commandDispatcher,
            IUsuarioRepository usuarioRepository) 

                : base(notificador, unitOfWork, commandDispatcher)
        {
            _usuarioRepository = usuarioRepository;
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

        public async Task<bool> Handle(CadastrarUsuarioCommand command, CancellationToken cancellationToken = default)
        {
            var usuario = new Usuario(command.Id, command.Nome, new Cpf(command.Cpf));

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

        public async Task<bool> Handle(AtualizarUsuarioCommand command, CancellationToken cancellationToken = default)
        {
            var usuario = await _usuarioRepository.ObterPorId(command.Id);

            usuario?.AlterarNome(command.Nome);

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

        public async Task<bool> Handle(ExcluirUsuarioCommand command, CancellationToken cancellationToken = default)
        {
            if (!await UsuarioAptoParaExcluir(command.Id))
                return false;

            await _usuarioRepository.Excluir(command.Id);

            await Commit();

            return CommandEhValido();
        }
    }
}
