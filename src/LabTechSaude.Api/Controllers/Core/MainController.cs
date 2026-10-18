using LabTechSaude.Api.Services.RateLimiting;
using LabTechSaude.Application.Commands.Dispatchers;
using LabTechSaude.Application.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabTechSaude.Api.Controllers.Core
{
    [Authorize]
    [ApiController]
    public abstract class MainController : Controller
    {
        protected readonly Notificador _notificador;
        protected readonly ICommandDispatcher _commandDispatcher;
        
        public MainController(
            Notificador notificador,
            ICommandDispatcher commandDispatcher)
        {
            _notificador = notificador;
            _commandDispatcher = commandDispatcher;
        }

        protected IActionResult CustomResponse(object? result = null)
        {
            if (OperacaoValida())
            {
                return Ok(result);
            }

            var problema = new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                { "mensagens", _notificador.Notificacoes.ToArray() }
            });

            if (HttpContext.Items.TryGetValue(RateLimitHttpItems.RetryAfterSegundos, out var retryAfter) && retryAfter is int segundos)
            {
                Response.Headers.RetryAfter = segundos.ToString();
                problema.Status = StatusCodes.Status429TooManyRequests;
                return StatusCode(StatusCodes.Status429TooManyRequests, problema);
            }

            return BadRequest(problema);
        }

        protected bool OperacaoValida() => !_notificador.ExistemNotificacoes();

        protected virtual bool ModelStateValida()
        {
            if (ModelState.IsValid)
                return true;

            NotificarErroModelInvalida();

            return false;
        }

        protected void NotificarErroModelInvalida()
        {
            var erros = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var erro in erros)
            {
                var erroMsg = erro.Exception == null ? erro.ErrorMessage : erro.Exception.Message;
                NotificarErro(erroMsg);
            }
        }

        protected void NotificarErro(string mensagem)
        {
            _notificador.IncluirNotificacao(mensagem);
        }

        protected void NotificarErros(List<string> mensagens)
        {
            foreach (var mensagem in mensagens)
            {
                NotificarErro(mensagem);
            }
        }
    }
}
