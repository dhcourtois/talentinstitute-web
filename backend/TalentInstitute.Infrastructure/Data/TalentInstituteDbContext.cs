using Microsoft.EntityFrameworkCore;
using TalentInstitute.Domain.Entities;
using TalentInstitute.Infrastructure.Data.Configurations;

namespace TalentInstitute.Infrastructure.Data;

public class TalentInstituteDbContext : DbContext
{
    public TalentInstituteDbContext(DbContextOptions<TalentInstituteDbContext> options) : base(options)
    {
    }

    public DbSet<Alumno> Alumnos { get; set; }
    public DbSet<Staff> Staff { get; set; }
    public DbSet<EntrevistaPadre> EntrevistasPadres { get; set; }
    public DbSet<Pace> Paces { get; set; }
    public DbSet<AlumnoPace> AlumnoPaces { get; set; }
    public DbSet<Meta> Metas { get; set; }
    public DbSet<Merito> Meritos { get; set; }
    public DbSet<ConfiguracionPrivilegios> ConfiguracionPrivilegios { get; set; }
    public DbSet<Anotacion> Anotaciones { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AlumnoConfiguration());
        modelBuilder.ApplyConfiguration(new StaffConfiguration());
        modelBuilder.ApplyConfiguration(new EntrevistaPadreConfiguration());
        modelBuilder.ApplyConfiguration(new PaceConfiguration());
        modelBuilder.ApplyConfiguration(new AlumnoPaceConfiguration());
        modelBuilder.ApplyConfiguration(new MetaConfiguration());
        modelBuilder.ApplyConfiguration(new MeritoConfiguration());
        modelBuilder.ApplyConfiguration(new ConfiguracionPrivilegiosConfiguration());
        modelBuilder.ApplyConfiguration(new AnotacionConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}
