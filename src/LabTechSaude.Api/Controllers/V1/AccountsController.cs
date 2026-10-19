using Asp.Versioning;
using LabTechSaude.Api.Controllers.Core;
using LabTechSaude.Api.Extensions;
using LabTechSaude.Api.ViewModels;
using LabTechSaude.Api.Configs.RateLimit;
using LabTechSaude.Api.Services.Accounts.Commands;
using LabTechSaude.Application.Commands.Dispatchers;
using LabTechSaude.Application.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LabTechSaude.Api.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/accounts")]
    public class AccountsController : MainController
    {
        public AccountsController(
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
        [HttpPost("redefinir-senha")]
        public async Task<IActionResult> RedefinirSenha([FromBody] RedefinirSenhaViewModel redefinirSenhaViewModel)
        {
            if (!ModelStateValida())
                return CustomResponse(ModelState);

            await _commandDispatcher.Send<RedefinirSenhaCommand, bool>(redefinirSenhaViewModel.ToCommand());

            return CustomResponse();
        }

        [AllowAnonymous]
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
