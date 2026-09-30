using LabTechSaude.Domain.Core.DomainObjects.Models;
using LabTechSaude.Domain.Core.DomainObjects.Validations;
using LabTechSaude.Domain.Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Domain.Pessoas
{
    public class Pessoa : Entity<Pessoa>
    {
        public string Nome { get; private set; } = string.Empty;
        public CPF CPF { get; set; } = new CPF(string.Empty);

        public Pessoa(Guid id, string nome, CPF cpf)
            : base(id)
        {
            CPF = cpf;

            AlterarNome(nome);
        }

        protected Pessoa() { }

        public void AlterarNome(string nome)
        {
            Nome = nome.Trim().ToUpper();
        }

        protected override EntityValidation<Pessoa> CriarValidacao() => new PessoaValidation();
    }
}
