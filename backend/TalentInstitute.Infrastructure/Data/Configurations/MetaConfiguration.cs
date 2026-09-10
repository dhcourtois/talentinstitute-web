using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Infrastructure.Data.Configurations;

public class MetaConfiguration : IEntityTypeConfiguration<Meta>
{
    public void Configure(EntityTypeBuilder<Meta> builder)
    {
        builder.ToTable("Metas");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.AlumnoPaceId).IsRequired();
        builder.Property(m => m.Turno).IsRequired().HasConversion<string>().HasMaxLength(10);
        builder.Property(m => m.PaginaInicial).IsRequired();
        builder.Property(m => m.PaginaFinal).IsRequired();
        builder.Property(m => m.FechaObjetivo).IsRequired().HasColumnType("date");
        builder.Property(m => m.PuntajeObtenido).HasColumnType("decimal(5,2)");
        builder.Property(m => m.Estado).IsRequired().HasConversion<string>().HasMaxLength(20);

        // Se calcula desde el rango; guardarla permitiría que se contradijeran.
        builder.Ignore(m => m.PaginasObjetivo);

        builder.HasOne<AlumnoPace>()
            .WithMany()
            .HasForeignKey(m => m.AlumnoPaceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
