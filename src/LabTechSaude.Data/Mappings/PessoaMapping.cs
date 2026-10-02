using LabTechSaude.Domain.Pessoas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Data.Mappings
{
    public class PessoaMapping : IEntityTypeConfiguration<Pessoa>
    {
        public void Configure(EntityTypeBuilder<Pessoa> builder)
        {
            builder.HasKey(pk => pk.Id);

            builder.Property(p => p.Nome)
                .HasMaxLength(100)
                .IsRequired();

            builder.OwnsOne(p => p.Cpf, c =>
            {
                c.Property(x => x.Value)
                    .HasColumnName("Cpf")
                    .HasMaxLength(11)
                    .IsRequired();

                c.HasIndex(x => x.Value)
                    .IsUnique();
            });

            builder.ToTable("Pessoas");
        }
    }
}
