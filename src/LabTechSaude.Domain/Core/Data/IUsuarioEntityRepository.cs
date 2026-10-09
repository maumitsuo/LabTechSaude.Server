using LabTechSaude.Domain.Core.DomainObjects.Models;
using LabTechSaude.Domain.Usuarios;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Domain.Core.Data
{
    public interface IUsuarioEntityRepository<TEntity> : IRepository<TEntity> where TEntity : UsuarioEntity<TEntity>
    {
        Task<bool> ExisteId(Guid id, Guid usuarioId);
        Task<TEntity?> ObterPorId(Guid id, Guid usuarioId);
        Task<IEnumerable<TEntity>> ObterTodos(Guid usuarioId);

        Task Excluir(Guid id, Guid usuarioId);
    }
}
