using System;
using FluentValidation;

namespace LabTechSaude.Domain.Core.ValueObjects
{
    public sealed class Email : IEquatable<Email>
    {
        public string Value { get; private set; } = string.Empty;

        public Email(string? value)
        {
            Value = Normalize(value);
        }

        public static string Normalize(string? value) =>
            string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

        public bool Equals(Email? other) =>
            other != null && string.Equals(Value, other.Value, StringComparison.Ordinal);

        public override bool Equals(object? obj) => obj is Email other && Equals(other);

        public static bool operator ==(Email? left, Email? right) => left?.Equals(right) ?? right is null;

        public static bool operator !=(Email? left, Email? right) => !(left == right);

        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

        public override string ToString() => Value;
    }

    public sealed class EmailValidation : AbstractValidator<Email>
    {
        public const int Email_MaxLength = 256;
        public const string Email_Required_Message = "Favor preencher o e-mail.";
        public const string Email_MaxLength_Message = "O e-mail deve ter no máximo 256 caracteres.";
        public const string Email_Invalid_Message = "E-mail inválido.";

        public EmailValidation()
        {
            RuleFor(email => email.Value)
                .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                        .WithMessage(Email_Required_Message)
                    .MaximumLength(Email_MaxLength)
                        .WithMessage(Email_MaxLength_Message)
                    .EmailAddress()
                        .WithMessage(Email_Invalid_Message);
        }
    }
}
