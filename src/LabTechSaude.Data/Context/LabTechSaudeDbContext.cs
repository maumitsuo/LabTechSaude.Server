using FluentValidation.Results;
using LabTechSaude.Domain.Core.DomainObjects.Models;
using LabTechSaude.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Data.Context
{
    public class LabTechSaudeDbContext : DbContext
    {
        public LabTechSaudeDbContext(
            DbContextOptions<LabTechSaudeDbContext> options)
            : base(options) 
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTrackingWithIdentityResolution;
            ChangeTracker.AutoDetectChangesEnabled = false;
            ChangeTracker.LazyLoadingEnabled = false;
        }

        public DbSet<Usuario> Usuarios { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<ValidationResult>();

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(LabTechSaudeDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
