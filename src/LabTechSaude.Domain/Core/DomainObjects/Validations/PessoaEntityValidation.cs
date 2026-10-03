using FluentValidation;
using LabTechSaude.Domain.Core.DomainObjects.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Domain.Core.DomainObjects.Validations
{
    public abstract class PessoaEntityValidation<T> : EntityValidation<T> where T : PessoaEntity<T>
    {
        public const string PessoaId_Required_Message = "Favor selecionar uma pessoa.";

        protected PessoaEntityValidation()
        {
            ValidarPessoa();
        }

        private void ValidarPessoa()
        {
            RuleFor(p => p.PessoaId)
                .NotEmpty()
                    .WithMessage(PessoaId_Required_Message);
        }
    }
}
