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

        public override async Task<bool> EhUnico(Usuario usuario)
        {
            return !await _context.Usuarios
                .AnyAsync(u => u.Cpf == usuario.Cpf &&
                               u.Id != usuario.Id);
        }
    }
}
