using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Infrastructure.Data.Configurations;

public class AnotacionConfiguration : IEntityTypeConfiguration<Anotacion>
{
    public void Configure(EntityTypeBuilder<Anotacion> builder)
    {
        builder.ToTable("Anotaciones");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.AlumnoId).IsRequired();
        builder.Property(a => a.StaffId).IsRequired();
        builder.Property(a => a.SemanaInicio).IsRequired().HasColumnType("date");
        builder.Property(a => a.Texto).IsRequired().HasMaxLength(Anotacion.LargoMaximo);
        builder.Property(a => a.FechaCreacion).IsRequired().HasColumnType("datetime2");

        builder.HasOne<Alumno>()
            .WithMany()
            .HasForeignKey(a => a.AlumnoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Staff>()
            .WithMany()
            .HasForeignKey(a => a.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        // La pantalla siempre pide "las anotaciones de este alumno, por semana".
        builder.HasIndex(a => new { a.AlumnoId, a.SemanaInicio });
    }
}
