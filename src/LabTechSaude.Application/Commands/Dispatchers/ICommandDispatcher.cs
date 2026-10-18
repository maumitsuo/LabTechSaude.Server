using LabTechSaude.Application.Commands.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Application.Commands.Dispatchers
{
    public interface ICommandDispatcher
    {
        Task<TResult> Send<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand;
    }
}
