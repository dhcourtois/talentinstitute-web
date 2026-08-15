using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Infrastructure.Data.Configurations;

public class ConfiguracionPrivilegiosConfiguration : IEntityTypeConfiguration<ConfiguracionPrivilegios>
{
    public void Configure(EntityTypeBuilder<ConfiguracionPrivilegios> builder)
    {
        builder.ToTable("ConfiguracionPrivilegios");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.FechaActualizacion).IsRequired().HasColumnType("datetime2");
        builder.Property(c => c.StaffIdActualizo);
        builder.Property(c => c.UmbralOficina).IsRequired();
        builder.Property(c => c.UmbralOficinaRevocado).IsRequired();
        builder.Property(c => c.UmbralComedor).IsRequired();
        builder.Property(c => c.UmbralComedorRevocado).IsRequired();
        builder.Property(c => c.UmbralPatio).IsRequired();
        builder.Property(c => c.UmbralPatioRevocado).IsRequired();
        builder.Property(c => c.UmbralBiblioteca).IsRequired();
        builder.Property(c => c.UmbralBibliotecaRevocado).IsRequired();
        builder.Property(c => c.UmbralActividades).IsRequired();
        builder.Property(c => c.UmbralActividadesRevocado).IsRequired();
    }
}
