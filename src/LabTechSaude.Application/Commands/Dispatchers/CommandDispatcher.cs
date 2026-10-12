using LabTechSaude.Application.Commands.Handlers;
using LabTechSaude.Application.Commands.Messages;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Application.Commands.Dispatchers
{
    public sealed class CommandDispatcher : ICommandDispatcher
    {
        private readonly IServiceProvider _services;
        
        public CommandDispatcher(IServiceProvider services)
        {
            _services = services;
        }

        public async Task<bool> Send<TCommand>(TCommand command) where TCommand : Command
        {
            var handler = _services.GetRequiredService<ICommandHandler<TCommand>>();

            return await handler.Handle(command);
        }
    }
}
