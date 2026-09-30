using System;
using FluentValidation;

namespace LabTechSaude.Domain.Core.ValueObjects
{
    public sealed class CPF : IEquatable<CPF>
    {
        private static readonly CPFValidator Validator = new CPFValidator();

        public string Value { get; }

        public CPF(string value)
        {
            var normalizedValue = CPFValidator.Normalize(value);
            var validationResult = Validator.Validate(value ?? string.Empty);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            Value = normalizedValue!;
        }

        public bool Equals(CPF? other) =>
            other != null && string.Equals(Value, other.Value, StringComparison.Ordinal);

        public override bool Equals(object? obj) => obj is CPF other && Equals(other);

        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

        public override string ToString() => Value;
    }
}
