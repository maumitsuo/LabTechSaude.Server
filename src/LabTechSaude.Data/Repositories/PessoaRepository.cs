using LabTechSaude.Data.Context;
using LabTechSaude.Data.Repositories.Core;
using LabTechSaude.Domain.Pessoas;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Data.Repositories
{
    public class PessoaRepository : Repository<Pessoa>, IPessoaRepository
    {
        public PessoaRepository(LabTechSaudeDbContext context) 
            : base(context)
        {
        }

        public override async Task<bool> EhUnico(Pessoa pessoa)
        {
            return !await _context.Pessoas
                .AnyAsync(p => p.Cpf == pessoa.Cpf &&
                               p.Id != pessoa.Id);
        }
    }
}
