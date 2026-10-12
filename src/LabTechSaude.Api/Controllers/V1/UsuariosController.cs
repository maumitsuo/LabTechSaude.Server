using Asp.Versioning;
using LabTechSaude.Api.Controllers.Core;
using LabTechSaude.Application.Commands.Dispatchers;
using LabTechSaude.Application.Extensions;
using LabTechSaude.Application.Notifications;
using LabTechSaude.Application.Services.Usuarios;
using LabTechSaude.Application.Services.Usuarios.Commands;
using LabTechSaude.Application.Services.Usuarios.Queries;
using LabTechSaude.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LabTechSaude.Api.Controllers.V1
{
    [ApiVersion("1.0", Deprecated = true)]
    [Route("api/v{version:apiVersion}/usuarios")]
    public class UsuariosController : MainController
    {
        private readonly IUsuarioQuery _usuarioQuery;

        public UsuariosController(
            Notificador notificador,
            ICommandDispatcher commandDispatcher,
            IUsuarioQuery usuarioQuery)
                : base(notificador, commandDispatcher)
        {
            _usuarioQuery = usuarioQuery;
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<UsuarioViewModel>> ObterPorId(Guid id)
        {
            var usuario = await _usuarioQuery.ObterPorId(id);

            if (usuario is null)
                return NotFound();

            return usuario!;
        }

        [HttpGet]
        public async Task<IEnumerable<UsuarioViewModel>> ObterTodos()
        {
            return await _usuarioQuery.ObterTodos();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UsuarioViewModel usuarioViewModel)
        {
            if (!ModelStateValida())
                return CustomResponse();

            var command = usuarioViewModel.ToCadastrarCommand();

            await _commandDispatcher.Send(command);

            return CustomResponse(command);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] UsuarioViewModel usuarioViewModel)
        {
            if (!ModelStateValida())
                return CustomResponse();

            var command = usuarioViewModel.ToAtualizarCommand();

            await _commandDispatcher.Send(command);

            return CustomResponse(command);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new ExcluirUsuarioCommand(id);

            await _commandDispatcher.Send(command);

            return CustomResponse(command);
        }
    }
}
