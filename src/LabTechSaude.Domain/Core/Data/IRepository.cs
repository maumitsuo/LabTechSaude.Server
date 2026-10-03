using LabTechSaude.Domain.Core.DomainObjects.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Domain.Core.Data
{
    public interface IRepository<TEntity> : IDisposable where TEntity : Entity<TEntity>
    {
        Task<bool> ExisteId(Guid id);
        Task<bool> EhUnico(TEntity entity);
        Task<TEntity?> ObterPorId(Guid id);
        Task<IEnumerable<TEntity>> ObterTodos();
        Task Cadastrar(TEntity entity);
        void Atualizar(TEntity entity);
        void Excluir(TEntity entity);
        Task Excluir(Guid id);
    }
}
