using Asp.Versioning;
using LabTechSaude.Api.Controllers.Core;
using LabTechSaude.Api.Extensions;
using LabTechSaude.Api.Services.Authentications.Commands;
using LabTechSaude.Api.ViewModels;
using LabTechSaude.Application.Commands.Dispatchers;
using LabTechSaude.Application.Notifications;
using LabTechSaude.Application.ViewModels;
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
        [HttpPost("cadastrar-usuario")]
        public async Task<IActionResult> Post([FromBody] CadastrarAspNetUserViewModel cadastrarAspNetUserViewModel)
        {
            if (!ModelStateValida())
                return CustomResponse();

            var command = cadastrarAspNetUserViewModel.ToCommand();

            await _commandDispatcher.Send(command);

            return CustomResponse(command);
        }


        [AllowAnonymous]
        [HttpPost("enviar-codigo-confirmar-email")]
        public async Task<IActionResult> EnviarCodigoConfirmarEmail([FromBody] EnviarCodigoConfirmarEmailViewModel enviarCodigoConfirmarEmail)
        {
            if (!ModelStateValida())
                return CustomResponse(ModelState);

            var command = new EnviarCodigoConfirmarEmailCommand(enviarCodigoConfirmarEmail.Email);

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
