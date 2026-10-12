using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Application.Commands.Messages
{
    public abstract class Command : ICommand
    {
        public string MessageType { get; protected set; }
        public DateTime Timestamp { get; private set; }
        
        protected Command()
        {
            MessageType = GetType().Name;
            Timestamp = DateTime.Now;
        }
    }
}
