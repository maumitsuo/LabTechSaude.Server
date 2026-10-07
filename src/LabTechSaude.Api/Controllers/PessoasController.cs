using Asp.Versioning;
using LabTechSaude.Api.Controllers.Core;
using LabTechSaude.Application.Notifications;
using LabTechSaude.Application.Services.Pessoas;
using LabTechSaude.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LabTechSaude.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class PessoasController : MainController
    {
        private readonly IPessoaService _pessoaService;

        public PessoasController(
            Notificador notificador,
            IPessoaService pessoaService)
            : base(notificador)
        {
            _pessoaService = pessoaService;
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<PessoaViewModel>> ObterPorId(Guid id)
        {
            var pessoa = await _pessoaService.ObterPorId(id);

            if (pessoa is null)
                return NotFound();

            return pessoa!;
        }

        [HttpGet]
        public async Task<IEnumerable<PessoaViewModel>> ObterTodos()
        {
            return await _pessoaService.ObterTodos();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PessoaViewModel pessoaViewModel)
        {
            if (!ModelStateValida())
                return CustomResponse();

            await _pessoaService.Cadastrar(pessoaViewModel);

            return CustomResponse(pessoaViewModel);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] PessoaViewModel pessoaViewModel)
        {
            if (!ModelStateValida())
                return CustomResponse();

            await _pessoaService.Atualizar(pessoaViewModel);

            return CustomResponse(pessoaViewModel);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _pessoaService.Excluir(id);

            return CustomResponse();
        }
    }
}
