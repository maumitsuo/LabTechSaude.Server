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
        private ValidationResult _validationResult => _validation.Value.Validate((T)this);

        public Guid Id { get; protected set; } = Guid.NewGuid();
        public bool IsValid => _validationResult.IsValid;
        public ICollection<string> ErrorMessages => _validationResult.Errors.Select(e => e.ErrorMessage).ToList();

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
