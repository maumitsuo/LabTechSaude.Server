using Asp.Versioning;
using LabTechSaude.Api.Controllers.Core;
using LabTechSaude.Application.Notifications;
using LabTechSaude.Application.Services.Usuarios;
using LabTechSaude.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LabTechSaude.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/usuarios")]
    public class UsuariosController : MainController
    {
        private readonly IUsuarioService _usuarioService;

        public UsuariosController(
            Notificador notificador,
            IUsuarioService usuarioService)
            : base(notificador)
        {
            _usuarioService = usuarioService;
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<UsuarioViewModel>> ObterPorId(Guid id)
        {
            var usuario = await _usuarioService.ObterPorId(id);

            if (usuario is null)
                return NotFound();

            return usuario!;
        }

        [HttpGet]
        public async Task<IEnumerable<UsuarioViewModel>> ObterTodos()
        {
            return await _usuarioService.ObterTodos();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UsuarioViewModel usuarioViewModel)
        {
            if (!ModelStateValida())
                return CustomResponse();

            await _usuarioService.Cadastrar(usuarioViewModel);

            return CustomResponse(usuarioViewModel);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] UsuarioViewModel usuarioViewModel)
        {
            if (!ModelStateValida())
                return CustomResponse();

            await _usuarioService.Atualizar(usuarioViewModel);

            return CustomResponse(usuarioViewModel);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _usuarioService.Excluir(id);

            return CustomResponse();
        }
    }
}
