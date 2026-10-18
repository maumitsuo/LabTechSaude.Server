using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace LabTechSaude.Api.Configs.Identity
{
    public sealed class EmailConfirmationTokenProviderOptions : IConfigureOptions<DataProtectionTokenProviderOptions>
    {
        public void Configure(DataProtectionTokenProviderOptions options)
        {
            options.TokenLifespan = TimeSpan.FromMinutes(10);
        }
    }
}
