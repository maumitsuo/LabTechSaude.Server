using LabTechSaude.Application.Commands.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace LabTechSaude.Application.Services.Usuarios.Commands
{
    public abstract class BaseUsuarioCommand : Command
    {
        public Guid Id { get; protected set; }
        public string Nome { get; protected set; } = string.Empty;
        public string Email { get; protected set; } = string.Empty;
        public string Cpf { get; protected set; } = string.Empty;
    }
}
