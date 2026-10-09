using LabTechSaude.Domain.Core.ValueObjects;
using LabTechSaude.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Data.Mappings
{
    public class UsuarioMapping : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.HasKey(pk => pk.Id);

            builder.Property(p => p.Nome)
                .HasMaxLength(UsuarioValidation.Nome_MaxLength)
                .IsRequired();

            builder.OwnsOne(p => p.Cpf, c =>
            {
                c.Property(x => x.Value)
                    .HasColumnName("Cpf")
                    .HasMaxLength(CpfValidator.Cpf_Length)
                    .IsFixedLength()
                    .IsRequired();

                c.HasIndex(x => x.Value)
                    .IsUnique();
            });

            builder.ToTable("Usuarios");
        }
    }
}
