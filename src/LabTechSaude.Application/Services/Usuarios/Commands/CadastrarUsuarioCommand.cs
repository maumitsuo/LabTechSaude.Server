using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Application.Services.Usuarios.Commands
{
    public class CadastrarUsuarioCommand : BaseUsuarioCommand
    {
        public CadastrarUsuarioCommand(Guid id, string nome, string email, string cpf)
        {
            Id = id;
            Nome = nome;
            Email = email;
            Cpf = cpf;
        }
    }
}
