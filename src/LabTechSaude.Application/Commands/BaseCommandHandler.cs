using FluentValidation.Results;
using LabTechSaude.Application.Commands.Dispatchers;
using LabTechSaude.Application.Notifications;
using LabTechSaude.Domain.Core.Data;
using LabTechSaude.Domain.Core.DomainObjects.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Application.Commands
{
    public abstract class BaseCommandHandler
    {
        protected readonly Notificador _notificador;
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly ICommandDispatcher _commandDispatcher;

        public BaseCommandHandler(
            Notificador notificador,
            IUnitOfWork unitOfWork,
            ICommandDispatcher commandDispatcher)
        {
            _notificador = notificador;
            _unitOfWork = unitOfWork;
            _commandDispatcher = commandDispatcher;
        }

        protected bool NotificarErro(string mensagem)
        {
            _notificador.IncluirNotificacao(mensagem);
            return false;
        }

        protected async Task ValidarEntidade<TEntity>(TEntity entity) where TEntity : Entity<TEntity>
        {
            if (entity.ValidationResult.IsValid)
                return;

            NotificarErrosValidacao(entity.ValidationResult);
        }

        private void NotificarErrosValidacao(ValidationResult validationResult)
        {
            foreach (var error in validationResult.Errors)
            {
                _notificador.IncluirNotificacao(error.ErrorMessage);
            }
        }

        protected bool CommandEhValido()
        {
            return !_notificador.ExistemNotificacoes();
        }

        protected virtual async Task<bool> Commit()
        {
            if (!CommandEhValido())
                return false;

            try
            {
                await _unitOfWork.Commit();

                return true;
            }
            catch (Exception)
            {
                return NotificarErro("Ocorreu um erro ao salvar os dados no banco.");
            }
        }
    }
}
