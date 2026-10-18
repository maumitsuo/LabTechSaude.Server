using Asp.Versioning;
using LabTechSaude.Api.Controllers.Core;
using LabTechSaude.Api.Extensions;
using LabTechSaude.Api.Models.Authentications;
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

            await _commandDispatcher.Send<CadastrarAspNetUserCommand, bool>(command);

            return CustomResponse();
        }


        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] EfetuarLoginViewModel efetuarLoginViewModel)
        {
            if (!ModelStateValida())
                return CustomResponse();

            var response = await _commandDispatcher.Send<EfetuarLoginCommand, LoginResponse?>(efetuarLoginViewModel.ToCommand());

            return CustomResponse(response);
        }

        [AllowAnonymous]
        [HttpPost("enviar-codigo-confirmar-email")]
        public async Task<IActionResult> EnviarCodigoConfirmarEmail([FromBody] EnviarCodigoConfirmarEmailViewModel enviarCodigoConfirmarEmail)
        {
            if (!ModelStateValida())
                return CustomResponse(ModelState);

            var command = enviarCodigoConfirmarEmail.ToCommand();

            await _commandDispatcher.Send<EnviarCodigoConfirmarEmailCommand, bool>(command);

            return CustomResponse();
        }

        [AllowAnonymous]
        [HttpPost("confirmar-codigo-email")]
        public async Task<IActionResult> ConfirmarCodigoEmail([FromBody] ConfirmarEmailViewModel confirmarEmail)
        {
            if (!ModelStateValida())
                return CustomResponse(ModelState);

            var command = confirmarEmail.ToCommand();

            await _commandDispatcher.Send<ConfirmarCodigoEmailCommand, bool>(command);

            return CustomResponse();
        }
    }
}
