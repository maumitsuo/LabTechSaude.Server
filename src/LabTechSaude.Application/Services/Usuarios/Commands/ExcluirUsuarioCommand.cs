using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Application.Services.Usuarios.Commands
{
    public class ExcluirUsuarioCommand : BaseUsuarioCommand
    {
        public ExcluirUsuarioCommand(Guid id)
        {
            Id = id;
        }
    }
}
