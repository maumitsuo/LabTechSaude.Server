using Asp.Versioning;
using LabTechSaude.Api.Controllers.Core;
using LabTechSaude.Application.Commands.Dispatchers;
using LabTechSaude.Application.Notifications;
using Microsoft.AspNetCore.Mvc;

namespace LabTechSaude.Api.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/authentications")]
    public class AuthenticationsController : MainController
    {
        public AuthenticationsController(
            Notificador notificador, 
            ICommandDispatcher commandDispatcher) 
                : base(notificador, commandDispatcher)
        {
        }
    }
}
