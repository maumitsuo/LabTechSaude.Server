using FluentValidation;
using LabTechSaude.Domain.Core.DomainObjects.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Domain.Core.DomainObjects.Validations
{
    public abstract class UsuarioEntityValidation<T> : EntityValidation<T> where T : UsuarioEntity<T>
    {
        public const string UsuarioId_Required_Message = "Favor selecionar um usuário.";

        protected UsuarioEntityValidation()
        {
            ValidarUsuario();
        }

        private void ValidarUsuario()
        {
            RuleFor(p => p.UsuarioId)
                .NotEmpty()
                    .WithMessage(UsuarioId_Required_Message);
        }
    }
}
