using System.Collections.Generic;
using System.Linq;
using FluentValidation;

namespace LabTechSaude.Domain.Core.ValueObjects
{
    public sealed class CPFValidator : AbstractValidator<string>
    {
        public CPFValidator()
        {
            RuleFor(value => value)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("CPF is required.")
                .Must(IsValid)
                .WithMessage("CPF is invalid.");
        }

        internal static string? Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var digits = new string(value.Where(IsAsciiDigit).ToArray());
            var ignoredCharacters = value.All(character =>
                IsAsciiDigit(character) || character == '.' || character == '-' || char.IsWhiteSpace(character));

            return ignoredCharacters && digits.Length == 11 ? digits : null;
        }

        private static bool IsValid(string? value)
        {
            var digits = Normalize(value);
            if (digits is null || digits.All(digit => digit == digits[0]))
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
