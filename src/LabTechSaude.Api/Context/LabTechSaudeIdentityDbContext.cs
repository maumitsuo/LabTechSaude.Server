using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LabTechSaude.Api.Context
{
    public class LabTechSaudeIdentityDbContext : IdentityDbContext, IDataProtectionKeyContext
    {
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;
        
        public LabTechSaudeIdentityDbContext(
            DbContextOptions<LabTechSaudeIdentityDbContext> options)
            : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(typeof(LabTechSaudeIdentityDbContext).Assembly);
            base.OnModelCreating(builder);
        }
    }
}
