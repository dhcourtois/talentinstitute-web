using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Infrastructure.Data.Configurations;

public class AlumnoPaceConfiguration : IEntityTypeConfiguration<AlumnoPace>
{
    public void Configure(EntityTypeBuilder<AlumnoPace> builder)
    {
        builder.ToTable("AlumnoPaces");
        builder.HasKey(ap => ap.Id);

        builder.Property(ap => ap.Materia)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ap => ap.Estado)
            .HasConversion<string>() // Enum a string
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(ap => ap.PuntajeFinal)
            .HasColumnType("decimal(5,2)");

        builder.Property(ap => ap.FechaInicio)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(ap => ap.FechaCompletado)
            .HasColumnType("datetime2");

        builder.Property(ap => ap.RowVersion)
            .IsRowVersion();

        builder.HasOne<Alumno>()
            .WithMany()
            .HasForeignKey(ap => ap.AlumnoId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne<Pace>()
            .WithMany()
            .HasForeignKey(ap => ap.PaceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
