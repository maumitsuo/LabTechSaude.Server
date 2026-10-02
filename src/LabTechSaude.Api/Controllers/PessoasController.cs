using Asp.Versioning;
using LabTechSaude.Api.Controllers.Core;
using LabTechSaude.Api.Extensions;
using LabTechSaude.Api.ViewModels;
using LabTechSaude.Domain.Pessoas;
using Microsoft.AspNetCore.Mvc;

namespace LabTechSaude.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class PessoasController : MainController
    {
        private readonly IPessoaRepository _pessoaRepository;

        public PessoasController(
            IPessoaRepository pessoaRepository)
        {
            _pessoaRepository = pessoaRepository;
        }

        [HttpGet]
        public async Task<IEnumerable<PessoaViewModel>> ObterTodos()
        {
            return (await _pessoaRepository
                .ObterTodos())
                .ToViewModel();
        }
    }
}
