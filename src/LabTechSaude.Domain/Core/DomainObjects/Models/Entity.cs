using FluentValidation.Results;
using LabTechSaude.Domain.Core.DomainObjects.Validations;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Domain.Core.DomainObjects.Models
{
    public abstract class Entity<T> where T : Entity<T>
    {
        private readonly Lazy<EntityValidation<T>> _validation;
        
        public Guid Id { get; protected set; } = Guid.NewGuid();
        public ValidationResult ValidationResult => _validation.Value.Validate((T)this);

        public Entity(Guid id)
        {
            Id = id;
            _validation = new Lazy<EntityValidation<T>>(CriarValidacao);
        }

        protected Entity()
        {
            _validation = new Lazy<EntityValidation<T>>(CriarValidacao);
        }

        protected abstract EntityValidation<T> CriarValidacao();
    }
}
