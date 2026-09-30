using FluentValidation;
using LabTechSaude.Domain.Core.DomainObjects.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Domain.Core.DomainObjects.Validations
{
    public class EntityValidation<T> : AbstractValidator<T> where T : Entity<T>
    {
    }
}
