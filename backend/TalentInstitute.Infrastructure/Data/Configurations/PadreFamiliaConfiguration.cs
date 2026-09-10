using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Infrastructure.Data.Configurations;

public class PadreFamiliaConfiguration : IEntityTypeConfiguration<PadreFamilia>
{
    public void Configure(EntityTypeBuilder<PadreFamilia> builder)
    {
        builder.ToTable("PadresFamilia");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Email).IsRequired().HasMaxLength(256);
        builder.Property(p => p.PasswordHash).IsRequired().HasMaxLength(512);
        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Activo).IsRequired();
        builder.Property(p => p.FechaCreacion).IsRequired().HasColumnType("datetime2");

        // El correo identifica la cuenta en el login; dos iguales harían
        // ambiguo a quién se autentica.
        builder.HasIndex(p => p.Email).IsUnique();
    }
}
