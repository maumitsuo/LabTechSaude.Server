using FluentValidation.Results;
using LabTechSaude.Application.Notifications;
using LabTechSaude.Domain.Core.Data;
using LabTechSaude.Domain.Core.DomainObjects.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Application.Services
{
    public abstract class BaseService
    {
        protected readonly Notificador _notificador;
        protected readonly IUnitOfWork _unitOfWork;

        public BaseService(
            Notificador notificador,
            IUnitOfWork unitOfWork)
        {
            _notificador = notificador;
            _unitOfWork = unitOfWork;
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

            if (await _unitOfWork.Commit())
                return true;

            return NotificarErro("Ocorreu um erro ao salvar os dados no banco.");
        }
    }
}
