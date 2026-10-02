using FluentValidation;
using LabTechSaude.Domain.Core.DomainObjects.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Domain.Core.DomainObjects.Validations
{
    public abstract class PessoaEntityValidation<T> : EntityValidation<T> where T : PessoaEntity<T>
    {
        protected PessoaEntityValidation()
        {
            ValidarPessoa();
        }

        private void ValidarPessoa()
        {
            RuleFor(p => p.PessoaId)
                .NotEmpty()
                    .WithMessage("Favor selecionar uma pessoa.");
        }
    }
}
