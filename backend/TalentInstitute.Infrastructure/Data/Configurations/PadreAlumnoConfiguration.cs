using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Infrastructure.Data.Configurations;

public class PadreAlumnoConfiguration : IEntityTypeConfiguration<PadreAlumno>
{
    public void Configure(EntityTypeBuilder<PadreAlumno> builder)
    {
        builder.ToTable("PadresAlumnos");
        builder.HasKey(pa => pa.Id);

        builder.Property(pa => pa.PadreFamiliaId).IsRequired();
        builder.Property(pa => pa.AlumnoId).IsRequired();
        builder.Property(pa => pa.FechaVinculo).IsRequired().HasColumnType("datetime2");

        builder.HasOne<PadreFamilia>()
            .WithMany()
            .HasForeignKey(pa => pa.PadreFamiliaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Alumno>()
            .WithMany()
            .HasForeignKey(pa => pa.AlumnoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Vincular dos veces al mismo hijo duplicaría filas y, con ellas, al
        // hijo en el portal. La base lo impide, no solo el caso de uso.
        builder.HasIndex(pa => new { pa.PadreFamiliaId, pa.AlumnoId }).IsUnique();
    }
}
