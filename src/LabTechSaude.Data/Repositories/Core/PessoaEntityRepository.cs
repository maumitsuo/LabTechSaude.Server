using LabTechSaude.Data.Context;
using LabTechSaude.Domain.Core.Data;
using LabTechSaude.Domain.Core.DomainObjects.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Data.Repositories.Core
{
    public abstract class PessoaEntityRepository<TEntity> : Repository<TEntity>, IPessoaEntityRepository<TEntity> where TEntity : PessoaEntity<TEntity>
    {
        protected PessoaEntityRepository(LabTechSaudeDbContext context) 
            : base(context)
        {
        }

        public virtual async Task<bool> ExisteId(Guid id, Guid pessoaId)
        {
            return await _context.Set<TEntity>()
                .AnyAsync(e => e.Id == id && e.PessoaId == pessoaId);
        }

        public virtual async Task<TEntity?> ObterPorId(Guid id, Guid pessoaId)
        {
            return await _context.Set<TEntity>()
                .AsNoTracking()
                .SingleOrDefaultAsync(e => e.Id == id && e.PessoaId == pessoaId);
        }

        public virtual async Task<IEnumerable<TEntity>> ObterTodos(Guid pessoaId)
        {
            return await _context.Set<TEntity>()
                .AsNoTracking()
                .Where(e => e.PessoaId == pessoaId)
                .ToListAsync();
        }

        public virtual async Task Excluir(Guid id, Guid pessoaId)
        {
            var entity = await ObterPorId(id, pessoaId);

            if (entity != null)
                Excluir(entity);
        }
    }
}
