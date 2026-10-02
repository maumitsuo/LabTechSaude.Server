using LabTechSaude.Data.Context;
using LabTechSaude.Domain.Core.Data;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Data.Uow
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly LabTechSaudeDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(LabTechSaudeDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Commit()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task BeginTransaction()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransaction()
        {
            try
            {
                await _context.SaveChangesAsync();

                if (_transaction is not null)
                    await _transaction.CommitAsync();
            }
            finally
            {
                if (_transaction is not null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task Rollback()
        {
            if (_transaction is not null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void ClearChangeTracker()
        {
            _context.ChangeTracker.Clear();
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
