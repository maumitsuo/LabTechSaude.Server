using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Application.Services.Usuarios.Commands
{
    public class AtualizarUsuarioCommand : BaseUsuarioCommand
    {
        public AtualizarUsuarioCommand(Guid id, string nome)
        {
            Id = id;
            Nome = nome;
        }
    }
}
