using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Infrastructure.Data.Configurations;

public class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> builder)
    {
        builder.ToTable("Staff");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Email).IsRequired().HasMaxLength(200);
        builder.HasIndex(s => s.Email).IsUnique();
        builder.Property(s => s.PasswordHash).IsRequired().HasMaxLength(500);
        builder.Property(s => s.Rol).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(s => s.Activo).IsRequired();
    }
}
