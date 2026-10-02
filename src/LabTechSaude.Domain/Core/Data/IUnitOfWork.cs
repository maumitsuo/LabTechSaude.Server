using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Domain.Core.Data
{
    public interface IUnitOfWork : IDisposable
    {
        Task<bool> Commit();
        Task BeginTransaction();
        Task CommitTransaction();
        Task Rollback();
        void ClearChangeTracker();
    }
}
