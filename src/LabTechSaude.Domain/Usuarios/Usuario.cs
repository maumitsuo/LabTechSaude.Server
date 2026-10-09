using LabTechSaude.Domain.Core.DomainObjects.Models;
using LabTechSaude.Domain.Core.DomainObjects.Validations;
using LabTechSaude.Domain.Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Domain.Usuarios
{
    public class Usuario : Entity<Usuario>
    {
        public string Nome { get; private set; } = string.Empty;
        public Cpf Cpf { get; private set; } = null!;

        public Usuario(Guid id, string nome, Cpf cpf)
            : base(id)
        {
            Cpf = cpf;

            AlterarNome(nome);
        }

        protected Usuario() { }

        public void AlterarNome(string nome)
        {
            Nome = nome.Trim().ToUpper();
        }

        protected override EntityValidation<Usuario> CriarValidacao() => new UsuarioValidation();
    }
}
