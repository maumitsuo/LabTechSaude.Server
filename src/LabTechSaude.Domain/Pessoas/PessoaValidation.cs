using FluentValidation;
using LabTechSaude.Domain.Core.DomainObjects.Validations;
using LabTechSaude.Domain.Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Domain.Pessoas
{
    public sealed class PessoaValidation : EntityValidation<Pessoa>
    {
        public PessoaValidation()
        {
            ValidarNome();
            ValidarCPF();
        }

        private void ValidarNome()
        {
            RuleFor(p => p.Nome)
                .Cascade(CascadeMode.Continue)
                    .NotEmpty()
                        .WithMessage("Favor preencher o nome.")
                    .Length(2, 100)
                        .WithMessage("O nome deve ter entre 2 e 100 caracteres.");
        }

        private void ValidarCPF()
        {
            RuleFor(p => p.Cpf)
                .SetValidator(new Cpf.CpfValidator());
        }
    }
}
