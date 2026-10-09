using LabTechSaude.Domain.Usuarios;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Domain.Core.DomainObjects.Models
{
    public abstract class UsuarioEntity<T> : Entity<T> where T : UsuarioEntity<T>
    {
        public Guid UsuarioId { get; protected set; } = Guid.Empty;
        public Usuario Usuario { get; protected set; } = null!;

        protected UsuarioEntity() {}

        public UsuarioEntity(Guid id, Guid usuarioId)
            : base(id)
        {
            UsuarioId = usuarioId;
        }
    }
}
