using System;
using FluentValidation;
using FluentValidation.Results;

namespace LabTechSaude.Domain.Core.ValueObjects
{
    public sealed class CPF : IEquatable<CPF>
    {
        public string Value { get; private set; } = string.Empty;

        public CPF(string value)
        {
            Value = CPFValidator.Normalize(value);
        }
        
        public bool Equals(CPF? other) =>
            other != null && string.Equals(Value, other.Value, StringComparison.Ordinal);

        public override bool Equals(object? obj) => obj is CPF other && Equals(other);

        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

        public override string ToString() => Value;

        public sealed class CPFValidator : AbstractValidator<CPF>
        {
            public CPFValidator()
            {
                RuleFor(p => p.Value)
                    .Cascade(CascadeMode.Stop)
                        .NotEmpty()
                            .WithMessage("Favor preencher o CPF.")
                        .Must(IsValid)
                            .WithMessage("CPF inválido.");
            }

            internal static string Normalize(string? value)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return string.Empty;
                }

                var digits = new string(value.Where(IsAsciiDigit).ToArray());
                var ignoredCharacters = value.All(character =>
                    IsAsciiDigit(character) || character == '.' || character == '-' || char.IsWhiteSpace(character));

                return ignoredCharacters && digits.Length == 11 ? digits : string.Empty;
            }

            private static bool IsValid(string? value)
            {
                var digits = Normalize(value);
                if (string.IsNullOrEmpty(digits) || digits.All(digit => digit == digits[0]))
                {
                    return false;
                }

                var numbers = digits.Select(character => character - '0').ToArray();
                var firstDigit = CalculateCheckDigit(numbers, 9);
                if (numbers[9] != firstDigit)
                {
                    return false;
                }

                return numbers[10] == CalculateCheckDigit(numbers, 10);
            }

            private static int CalculateCheckDigit(IReadOnlyList<int> numbers, int length)
            {
                var sum = 0;
                for (var index = 0; index < length; index++)
                {
                    sum += numbers[index] * (length + 1 - index);
                }

                var remainder = sum % 11;
                return remainder < 2 ? 0 : 11 - remainder;
            }

            private static bool IsAsciiDigit(char character) => character >= '0' && character <= '9';
        }
    }
}
