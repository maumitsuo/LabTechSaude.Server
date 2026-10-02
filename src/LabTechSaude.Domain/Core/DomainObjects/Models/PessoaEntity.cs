using LabTechSaude.Domain.Pessoas;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Domain.Core.DomainObjects.Models
{
    public abstract class PessoaEntity<T> : Entity<T> where T : PessoaEntity<T>
    {
        public Guid PessoaId { get; protected set; } = Guid.Empty;
        public Pessoa Pessoa { get; protected set; } = null!;

        protected PessoaEntity() {}

        public PessoaEntity(Guid id, Guid pessoaId)
            : base(id)
        {
            PessoaId = pessoaId;
        }
    }
}
