using LabTechSaude.Application.Notifications;
using Microsoft.AspNetCore.Mvc;

namespace LabTechSaude.Api.Controllers.Core
{
    [ApiController]
    public abstract class MainController : Controller
    {
        protected readonly Notificador _notificador;

        public MainController(Notificador notificador)
        {
            _notificador = notificador;
        }

        protected IActionResult CustomResponse(object? result = null)
        {
            if (OperacaoValida())
            {
                return Ok(result);
            }

            
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                { "mensagens", _notificador.Notificacoes.ToArray() }
            }));
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
