using FluentValidation;
using LabTechSaude.Domain.Core.DomainObjects.Validations;
using LabTechSaude.Domain.Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Domain.Usuarios
{
    public sealed class UsuarioValidation : EntityValidation<Usuario>
    {
        public const string Nome_Required_Message = "Favor preencher o nome.";
        public const int Nome_MinLength = 2;
        public const int Nome_MaxLength = 256;
        public const string Nome_Length_Message = "O nome deve ter entre 2 e 256 caracteres.";
        public const string Unique_Message = "Já existe um usuário cadastrado com esse CPF.";
        public const string NotFound_Message = "Usuário não encontrado.";

        public UsuarioValidation()
        {
            ValidarNome();
            ValidarCPF();
        }

        private void ValidarNome()
        {
            RuleFor(p => p.Nome)
                .Cascade(CascadeMode.Continue)
                    .NotEmpty()
                        .WithMessage(Nome_Required_Message)
                    .Length(Nome_MinLength, Nome_MaxLength)
                        .WithMessage(Nome_Length_Message);
        }

        private void ValidarCPF()
        {
            RuleFor(p => p.Cpf)
                .SetValidator(new CpfValidator());
        }
    }
}
