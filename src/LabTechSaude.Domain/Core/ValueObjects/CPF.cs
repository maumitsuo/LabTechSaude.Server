using System;
using FluentValidation;
using FluentValidation.Results;

namespace LabTechSaude.Domain.Core.ValueObjects
{
    public sealed class Cpf : IEquatable<Cpf>
    {
        private static bool IsAsciiDigit(char character) => character >= '0' && character <= '9';

        public string Value { get; private set; } = string.Empty;

        public Cpf(string value)
        {
            Value = Normalize(value);
        }

        public static string Normalize(string? value)
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

        public bool Equals(Cpf? other) =>
            other != null && string.Equals(Value, other.Value, StringComparison.Ordinal);

        public override bool Equals(object? obj) => obj is Cpf other && Equals(other);

        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

        public override string ToString() => $"{Value:000\\.000\\.000\\-00}";
    }

    public sealed class CpfValidator : AbstractValidator<Cpf>
    {
        public const string Cpf_Required_Message = "Favor preencher o CPF.";
        public const string Cpf_Invalid_Message = "CPF inválido.";

        public CpfValidator()
        {
            RuleFor(p => p.Value)
                .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                        .WithMessage(Cpf_Required_Message)
                    .Must(IsValid)
                        .WithMessage(Cpf_Invalid_Message);
        }

        private static bool IsValid(string? value)
        {
            var digits = Cpf.Normalize(value);
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
    }
}
