using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Security.Cryptography;

namespace LabTechSaude.Api.Configs.Identity
{
    public sealed class EmailConfirmationTokenProvider : DataProtectorTokenProvider<IdentityUser>
    {
        public const string ProviderName = "EmailConfirmationCode";

        private readonly IPasswordHasher<IdentityUser> _passwordHasher;

        public EmailConfirmationTokenProvider(
            IDataProtectionProvider dataProtectionProvider,
            IOptions<DataProtectionTokenProviderOptions> options,
            ILogger<DataProtectorTokenProvider<IdentityUser>> logger,
            IPasswordHasher<IdentityUser> passwordHasher)
            : base(dataProtectionProvider, options, logger)
        {
            _passwordHasher = passwordHasher;
        }

        public override async Task<string> GenerateAsync(
            string purpose,
            UserManager<IdentityUser> manager,
            IdentityUser user)
        {
            var protectedToken = await base.GenerateAsync(purpose, manager, user);
            var code = RandomNumberGenerator.GetInt32(0, 1_000_000)
                .ToString("D6", CultureInfo.InvariantCulture);
            var hashedCode = _passwordHasher.HashPassword(user, code);
            var tokenValue = $"{hashedCode}|{protectedToken}";
            var result = await manager.SetAuthenticationTokenAsync(
                user,
                ProviderName,
                purpose,
                tokenValue);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Não foi possível armazenar o código de confirmação: {string.Join("; ", result.Errors.Select(error => error.Description))}");
            }

            return code;
        }

        public override async Task<bool> ValidateAsync(
            string purpose,
            string token,
            UserManager<IdentityUser> manager,
            IdentityUser user)
        {
            if (token.Length != 6 || token.Any(character => character is < '0' or > '9'))
                return false;

            var tokenValue = await manager.GetAuthenticationTokenAsync(user, ProviderName, purpose);
            if (string.IsNullOrEmpty(tokenValue))
                return false;

            var separatorIndex = tokenValue.IndexOf('|');
            if (separatorIndex <= 0 || separatorIndex == tokenValue.Length - 1)
                return false;

            var hashedCode = tokenValue[..separatorIndex];
            var protectedToken = tokenValue[(separatorIndex + 1)..];
            if (_passwordHasher.VerifyHashedPassword(user, hashedCode, token) == PasswordVerificationResult.Failed
                || !await base.ValidateAsync(purpose, protectedToken, manager, user))
            {
                return false;
            }

            var result = await manager.RemoveAuthenticationTokenAsync(user, ProviderName, purpose);
            return result.Succeeded;
        }
    }
}
