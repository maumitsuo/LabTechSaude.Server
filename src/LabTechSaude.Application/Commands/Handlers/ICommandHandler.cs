using LabTechSaude.Application.Commands.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Application.Commands.Handlers
{
    public interface ICommandHandler<in TCommand> where TCommand : Command
    {
        Task<bool> Handle(TCommand command, CancellationToken cancellationToken = default);
    }
}
