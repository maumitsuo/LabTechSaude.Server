using LabTechSaude.Application.Commands.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Application.Commands.Handlers
{
    public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand
    {
        Task<TResult> Handle(TCommand command, CancellationToken cancellationToken = default);
    }
}
