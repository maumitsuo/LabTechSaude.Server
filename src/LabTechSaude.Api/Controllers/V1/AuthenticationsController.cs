using Asp.Versioning;
using LabTechSaude.Api.Controllers.Core;
using LabTechSaude.Api.Services.Authentications.Commands;
using LabTechSaude.Api.ViewModels;
using LabTechSaude.Application.Commands.Dispatchers;
using LabTechSaude.Application.Notifications;
using Microsoft.AspNetCore.Authorization;
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

        [AllowAnonymous]
        [HttpPost("enviar-codigo-confirmar-email")]
        public async Task<IActionResult> EnviarCodigoConfirmarEmail([FromBody] EnviarCodigoConfirmarEmailViewModel enviarCodigoConfirmarEmail)
        {
            if (!ModelStateValida())
                return CustomResponse(ModelState);

            var command = new EnviarCodigoConfirmarEmailCommand(enviarCodigoConfirmarEmail.UsuarioId);

            await _commandDispatcher.Send(command);

            return CustomResponse();
        }

        [AllowAnonymous]
        [HttpPost("confirmar-codigo-email")]
        public async Task<IActionResult> ConfirmarCodigoEmail([FromBody] ConfirmarEmailViewModel confirmarEmail)
        {
            if (!ModelStateValida())
                return CustomResponse(ModelState);

            var command = new ConfirmarCodigoEmailCommand(confirmarEmail.Email, confirmarEmail.Code);

            await _commandDispatcher.Send(command);

            return CustomResponse();
        }
    }
}
