using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Infrastructure.Data.Configurations;

public class MeritoConfiguration : IEntityTypeConfiguration<Merito>
{
    public void Configure(EntityTypeBuilder<Merito> builder)
    {
        builder.ToTable("Meritos");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.AlumnoId).IsRequired();
        builder.Property(m => m.StaffId).IsRequired();
        builder.Property(m => m.Tipo).IsRequired().HasConversion<string>().HasMaxLength(10);
        builder.Property(m => m.Puntos).IsRequired();
        builder.Property(m => m.Motivo).IsRequired().HasColumnType("NVARCHAR(MAX)");
        builder.Property(m => m.FechaAplicado).IsRequired().HasColumnType("datetime2");
        builder.Property(m => m.Revocado).IsRequired();
        builder.Property(m => m.FechaRevocacion).HasColumnType("datetime2");
        builder.Property(m => m.RowVersion).IsRowVersion();

        builder.HasOne<Alumno>()
            .WithMany()
            .HasForeignKey(m => m.AlumnoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Staff>()
            .WithMany()
            .HasForeignKey(m => m.StaffId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
