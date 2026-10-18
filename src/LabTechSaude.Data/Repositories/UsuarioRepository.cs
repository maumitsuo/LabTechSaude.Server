using LabTechSaude.Data.Context;
using LabTechSaude.Data.Repositories.Core;
using LabTechSaude.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Data.Repositories
{
    public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
    {
        public UsuarioRepository(LabTechSaudeDbContext context) 
            : base(context)
        {
        }

        public async Task<Usuario?> ObterPorEmail(string email)
        {
            return await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.Value == email);
        }

        public override async Task<bool> EhUnico(Usuario usuario)
        {
            return !await _context.Usuarios
                .AnyAsync(u => (u.Cpf.Value == usuario.Cpf.Value ||
                                u.Email.Value == usuario.Email.Value) &&
                               u.Id != usuario.Id);
        }
    }
}
