using LabTechSaude.Data.Context;
using LabTechSaude.Domain.Core.Data;
using LabTechSaude.Domain.Core.DomainObjects.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Data.Repositories.Core
{
    public abstract class Repository<TEntity> : IRepository<TEntity> where TEntity : Entity<TEntity>
    {
        protected LabTechSaudeDbContext _context;

        protected Repository(LabTechSaudeDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExisteId(Guid id)
        {
            return await _context.Set<TEntity>().AnyAsync(e => e.Id == id);
        }

        public abstract Task<bool> EhUnico(TEntity entity);

        public async virtual Task<TEntity?> ObterPorId(Guid id)
        {
            return await _context.Set<TEntity>()
                .AsNoTracking()
                .SingleOrDefaultAsync(e => e.Id == id);
        }

        public async virtual Task<IEnumerable<TEntity>> ObterTodos()
        {
            return await _context.Set<TEntity>()
                .AsNoTracking()
                .ToListAsync();
        }

        public async virtual Task Cadastrar(TEntity entity)
        {
            await _context.Set<TEntity>().AddAsync(entity);
        }

        public virtual void Atualizar(TEntity entity)
        {
            _context.Set<TEntity>().Update(entity);
        }

        public virtual void Excluir(TEntity entity)
        {
            _context.Set<TEntity>().Remove(entity);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
