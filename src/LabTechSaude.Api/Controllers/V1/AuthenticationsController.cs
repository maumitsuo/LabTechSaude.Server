using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;
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
using LabTechSaude.Api.Configs.RateLimit;

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
        [EnableRateLimiting(RateLimiterConfig.AutenticacaoIpPolicy)]
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
        [EnableRateLimiting(RateLimiterConfig.AutenticacaoIpPolicy)]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] EfetuarLoginViewModel efetuarLoginViewModel)
        {
            if (!ModelStateValida())
                return CustomResponse();

            var response = await _commandDispatcher.Send<EfetuarLoginCommand, LoginResponse?>(efetuarLoginViewModel.ToCommand());

            return CustomResponse(response);
        }

        [AllowAnonymous]
        [EnableRateLimiting(RateLimiterConfig.AutenticacaoIpPolicy)]
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
        [EnableRateLimiting(RateLimiterConfig.AutenticacaoIpPolicy)]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenViewModel refreshTokenViewModel)
        {
            if (!ModelStateValida())
                return CustomResponse();

            await _commandDispatcher.Send<RevogarRefreshTokenCommand, bool>(
                new RevogarRefreshTokenCommand(refreshTokenViewModel.RefreshToken));

            return CustomResponse();
        }

        [AllowAnonymous]
        [EnableRateLimiting(RateLimiterConfig.EnviarEmailIdentityPolicy)]
        [HttpPost("enviar-confirmar-email")]
        public async Task<IActionResult> EnviarConfirmarEmail([FromBody] EnviarConfirmarEmailViewModel enviarCodigoConfirmarEmail)
        {
            if (!ModelStateValida())
                return CustomResponse(ModelState);

            var command = enviarCodigoConfirmarEmail.ToCommand();

            await _commandDispatcher.Send<EnviarConfirmarEmailCommand, bool>(command);

            return CustomResponse();
        }

        [AllowAnonymous]
        [EnableRateLimiting(RateLimiterConfig.EnviarEmailIdentityPolicy)]
        [HttpPost("esqueci-minha-senha")]
        public async Task<IActionResult> EsqueciMinhaSenha([FromBody] EsqueciMinhaSenhaViewModel esqueciMinhaSenhaViewModel)
        {
            if (!ModelStateValida())
                return CustomResponse(ModelState);

            await _commandDispatcher.Send<EsqueciMinhaSenhaCommand, bool>(esqueciMinhaSenhaViewModel.ToCommand());

            return CustomResponse();
        }

        [AllowAnonymous]
        [EnableRateLimiting(RateLimiterConfig.AutenticacaoIpPolicy)]
        [HttpPost("redefinir-senha")]
        public async Task<IActionResult> RedefinirSenha([FromBody] RedefinirSenhaViewModel redefinirSenhaViewModel)
        {
            if (!ModelStateValida())
                return CustomResponse(ModelState);

            await _commandDispatcher.Send<RedefinirSenhaCommand, bool>(redefinirSenhaViewModel.ToCommand());

            return CustomResponse();
        }

        [AllowAnonymous]
        [EnableRateLimiting(RateLimiterConfig.AutenticacaoIpPolicy)]
        [HttpPost("confirmar-email")]
        public async Task<IActionResult> ConfirmarEmail([FromBody] ConfirmarEmailViewModel confirmarEmail)
        {
            if (!ModelStateValida())
                return CustomResponse(ModelState);

            var command = confirmarEmail.ToCommand();

            await _commandDispatcher.Send<ConfirmarEmailCommand, bool>(command);

            return CustomResponse();
        }
    }
}
