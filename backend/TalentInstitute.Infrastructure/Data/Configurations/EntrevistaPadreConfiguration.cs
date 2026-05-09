using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Infrastructure.Data.Configurations;

public class EntrevistaPadreConfiguration : IEntityTypeConfiguration<EntrevistaPadre>
{
    public void Configure(EntityTypeBuilder<EntrevistaPadre> builder)
    {
        builder.ToTable("EntrevistasPadres");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.NombrePadre)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Comentarios)
            .HasMaxLength(1000);
    }
}
