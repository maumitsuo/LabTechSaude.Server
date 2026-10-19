using Asp.Versioning;
using LabTechSaude.Api.Controllers.Core;
using LabTechSaude.Api.Extensions;
using LabTechSaude.Api.Models.Authentications;
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
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] EfetuarLoginViewModel efetuarLoginViewModel)
        {
            if (!ModelStateValida())
                return CustomResponse();

            var response = await _commandDispatcher.Send<EfetuarLoginCommand, LoginResponse?>(efetuarLoginViewModel.ToCommand());

            return CustomResponse(response);
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenViewModel refreshTokenViewModel)
        {
            if (!ModelStateValida())
                return CustomResponse();

            var response = await _commandDispatcher.Send<RenovarRefreshTokenCommand, LoginResponse?>(
                new RenovarRefreshTokenCommand(refreshTokenViewModel.RefreshToken));

            return CustomResponse(response);
        }

        [AllowAnonymous]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenViewModel refreshTokenViewModel)
        {
            if (!ModelStateValida())
                return CustomResponse();

            await _commandDispatcher.Send<RevogarRefreshTokenCommand, bool>(
                new RevogarRefreshTokenCommand(refreshTokenViewModel.RefreshToken));

            return CustomResponse();
        }
    }
}
