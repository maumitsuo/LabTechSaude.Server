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
        [HttpPost("enviar-confirmar-email")]
        public async Task<IActionResult> EnviarConfirmarEmail([FromBody] EnviarCodigoConfirmarEmailViewModel enviarConfirmarEmail)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            var command = new EnviarCodigoConfirmarEmailCommand(enviarConfirmarEmail.UsuarioId);

            await _commandDispatcher.Send(command);

            return CustomResponse(command);
        }

        [AllowAnonymous]
        [HttpPost("confirmar-email")]
        public async Task<IActionResult> ConfirmarEmail([FromBody] ConfirmarEmailViewModel confirmarEmail)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            var command = new ConfirmarEmailCommand(confirmarEmail.Email, confirmarEmail.Code);

            await _commandDispatcher.Send(command);

            return CustomResponse(command);
        }
    }
}
