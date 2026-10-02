using LabTechSaude.Domain.Core.DomainObjects.Models;
using LabTechSaude.Domain.Pessoas;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Domain.Core.Data
{
    public interface IPessoaEntityRepository<TEntity> : IRepository<TEntity> where TEntity : PessoaEntity<TEntity>
    {
        Task<bool> ExisteId(Guid id, Guid pessoaId);
        Task<TEntity?> ObterPorId(Guid id, Guid pessoaId);
        Task<IEnumerable<TEntity>> ObterTodos(Guid pessoaId);
    }
}
