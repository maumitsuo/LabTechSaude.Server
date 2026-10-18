using LabTechSaude.Api.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LabTechSaude.Api.Configs.Identity
{
    public static class IdentityConfig
    {
        private const int MinimoBytesSecretKey = 32;

        public static WebApplicationBuilder AddIdentityConfig(this WebApplicationBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            var jwtTokenOptionsSection = builder.Configuration.GetSection("JwtTokenOptions");
            builder.Services.AddOptions<JwtOptions>()
                .Bind(jwtTokenOptionsSection)
                .Validate(
                    options => options.RefreshTokenSlidingExpirationDays > 0
                        && options.RefreshTokenAbsoluteExpirationDays >= options.RefreshTokenSlidingExpirationDays,
                    "A validade absoluta do refresh token deve ser maior ou igual à validade deslizante.")
                .Validate(
                    options => !string.IsNullOrEmpty(options.SecretKey)
                        && System.Text.Encoding.ASCII.GetByteCount(options.SecretKey) >= MinimoBytesSecretKey
                        && options.SecretKey.All(char.IsAscii),
                    $"JwtOptions:SecretKey deve conter apenas caracteres ASCII e ter no mínimo {MinimoBytesSecretKey} caracteres.")
                .ValidateOnStart();
            var jwtOptions = jwtTokenOptionsSection.Get<JwtOptions>();

            builder.Services.AddDbContext<LabTechSaudeIdentityDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
           
            // Chaves compartilhadas no banco para que todas as instâncias validem os mesmos tokens
            builder.Services.AddDataProtection()
                .SetApplicationName("LabTechSaude")
                .PersistKeysToDbContext<LabTechSaudeIdentityDbContext>();

            builder.Services.ConfigureOptions<EmailConfirmationTokenProviderOptions>();

            //AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddErrorDescriber<IdentityMensagensPortugues>()
                .AddEntityFrameworkStores<LabTechSaudeIdentityDbContext>()
                .AddDefaultTokenProviders()
                .AddTokenProvider<EmailConfirmationTokenProvider>(EmailConfirmationTokenProvider.ProviderName);

            builder.Services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;
                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
                // User settings
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;

                options.SignIn.RequireConfirmedEmail = true;
                options.Tokens.EmailConfirmationTokenProvider = EmailConfirmationTokenProvider.ProviderName;
            });

            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        System.Text.Encoding.ASCII.GetBytes(jwtOptions?.SecretKey ?? throw new InvalidOperationException("JWT Secret is not configured."))),
                   
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions?.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwtOptions?.Audience,

                    ValidateLifetime = true
                };
            });

            return builder;
        }
    }
}
