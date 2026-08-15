using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Domain.Entities;
using TalentInstitute.Infrastructure.Data;

namespace TalentInstitute.API.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TalentInstituteDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        // MigrateAsync se llama en Program.cs para todos los entornos.
        // Aquí solo sembramos datos iniciales de prueba.

        // Cada sección llama su propio SaveChangesAsync para evitar que EF
        // acumule inserts de secciones distintas y genere batches duplicados.

        // 1. Configuración de Privilegios
        if (!await context.ConfiguracionPrivilegios.AnyAsync())
        {
            var config = new ConfiguracionPrivilegios(
                umbralOficina: 0,        umbralOficinaRevocado: -2,
                umbralComedor: 0,        umbralComedorRevocado: -3,
                umbralPatio: -1,         umbralPatioRevocado: -5,
                umbralBiblioteca: 3,     umbralBibliotecaRevocado: 0,
                umbralActividades: 5,    umbralActividadesRevocado: 2
            );
            context.ConfiguracionPrivilegios.Add(config);
            await context.SaveChangesAsync();
        }

        // 2. Staff semilla (Principal, Supervisora, Monitora)
        if (!await context.Staff.AnyAsync())
        {
            context.Staff.AddRange(
                new Staff("principal@talentinstitute.com",   hasher.Hash("principal123"),   Rol.Principal),
                new Staff("supervisora@talentinstitute.com", hasher.Hash("supervisora123"), Rol.Supervisora),
                new Staff("monitora@talentinstitute.com",    hasher.Hash("monitora123"),    Rol.Monitora)
            );
            await context.SaveChangesAsync();
        }

        // 3. Catálogo de PACEs
        if (!await context.Paces.AnyAsync())
        {
            var mat1045 = new Pace("MAT", 1045, 100, 80);
            var mat1046 = new Pace("MAT", 1046, 100, 80);
            var esp1045 = new Pace("ESP", 1045, 100, 80);
            var esp1046 = new Pace("ESP", 1046, 100, 80);

            context.Paces.AddRange(mat1045, mat1046, esp1045, esp1046);
            await context.SaveChangesAsync();

            // 4. Alumnos (solo si los PACEs son nuevos)
            if (!await context.Alumnos.AnyAsync())
            {
                var mateo = new Alumno("MAT-001", "Mateo", "Fernández",  "4 Primaria");
                var sofia = new Alumno("MAT-002", "Sofía",  "Martínez",  "3 Primaria");

                context.Alumnos.AddRange(mateo, sofia);
                await context.SaveChangesAsync();

                // 5. Asignación de PACEs
                var mateoPace = new AlumnoPace(mateo.Id, mat1045.Id, "MAT");
                var sofiaPace = new AlumnoPace(sofia.Id, mat1045.Id, "MAT");

                context.AlumnoPaces.AddRange(mateoPace, sofiaPace);
                await context.SaveChangesAsync();

                // 6. Meta diaria de ejemplo para Mateo
                var metaMateo = new Meta(mateoPace.Id, Turno.Mañana, 5, DateOnly.FromDateTime(DateTime.UtcNow));
                context.Metas.Add(metaMateo);

                mateoPace.RegistrarPrimeraMeta();
                context.AlumnoPaces.Update(mateoPace);

                await context.SaveChangesAsync();
            }
        }
    }
}
