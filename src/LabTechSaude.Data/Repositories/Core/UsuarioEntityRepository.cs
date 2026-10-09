using LabTechSaude.Data.Context;
using LabTechSaude.Domain.Core.Data;
using LabTechSaude.Domain.Core.DomainObjects.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Data.Repositories.Core
{
    public abstract class UsuarioEntityRepository<TEntity> : Repository<TEntity>, IUsuarioEntityRepository<TEntity> where TEntity : UsuarioEntity<TEntity>
    {
        protected UsuarioEntityRepository(LabTechSaudeDbContext context) 
            : base(context)
        {
        }

        public virtual async Task<bool> ExisteId(Guid id, Guid usuarioId)
        {
            return await _context.Set<TEntity>()
                .AnyAsync(e => e.Id == id && e.UsuarioId == usuarioId);
        }

        public virtual async Task<TEntity?> ObterPorId(Guid id, Guid usuarioId)
        {
            return await _context.Set<TEntity>()
                .AsNoTracking()
                .SingleOrDefaultAsync(e => e.Id == id && e.UsuarioId == usuarioId);
        }

        public virtual async Task<IEnumerable<TEntity>> ObterTodos(Guid usuarioId)
        {
            return await _context.Set<TEntity>()
                .AsNoTracking()
                .Where(e => e.UsuarioId == usuarioId)
                .ToListAsync();
        }

        public virtual async Task Excluir(Guid id, Guid usuarioId)
        {
            var entity = await ObterPorId(id, usuarioId);

            if (entity != null)
                Excluir(entity);
        }
    }
}
