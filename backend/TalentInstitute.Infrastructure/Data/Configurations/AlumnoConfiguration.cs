using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Infrastructure.Data.Configurations;

public class AlumnoConfiguration : IEntityTypeConfiguration<Alumno>
{
    public void Configure(EntityTypeBuilder<Alumno> builder)
    {
        builder.ToTable("Alumnos");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.NumeroMatricula)
            .HasMaxLength(50);

        builder.Property(a => a.Nombre)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Apellido)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Nivel)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.BalanceMeritos)
            .IsRequired();

        builder.Property(a => a.RowVersion)
            .IsRowVersion();

        // PrivilegeStatus as Owned Type
        builder.OwnsOne(a => a.PrivilegeStatus, ps =>
        {
            ps.Property(p => p.Oficina).HasColumnName("Privilegio_Oficina");
            ps.Property(p => p.Comedor).HasColumnName("Privilegio_Comedor");
            ps.Property(p => p.Patio).HasColumnName("Privilegio_Patio");
            ps.Property(p => p.Biblioteca).HasColumnName("Privilegio_Biblioteca");
            ps.Property(p => p.Actividades).HasColumnName("Privilegio_Actividades");
        });
    }
}
