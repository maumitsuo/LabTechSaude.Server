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
        public Email Email { get; private set; } = new Email(string.Empty);
        public Cpf Cpf { get; private set; } = new Cpf(string.Empty);
        

        public Usuario(Guid id, string nome, Email email, Cpf cpf)
            : base(id)
        {
            Cpf = cpf;
            Email = email;
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
