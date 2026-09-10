using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Infrastructure.Data.Configurations;

public class PaceConfiguration : IEntityTypeConfiguration<Pace>
{
    public void Configure(EntityTypeBuilder<Pace> builder)
    {
        builder.ToTable("Paces");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Materia)
            .IsRequired()
            .HasMaxLength(50);

        // Alfanumérico: además de 1045 el colegio maneja códigos como RR01.
        builder.Property(p => p.Numero)
            .IsRequired()
            .HasMaxLength(Pace.LargoMaximoNumero);

        // Nulo en los PACEs capturados antes del issue #6.
        builder.Property(p => p.TotalPaginas);
    }
}
