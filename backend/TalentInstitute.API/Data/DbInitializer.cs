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

        // Aplicar migraciones automáticas en desarrollo
        await context.Database.MigrateAsync();

        // 1. Semilla de Configuración de Privilegios
        if (!await context.ConfiguracionPrivilegios.AnyAsync())
        {
            var config = new ConfiguracionPrivilegios(
                umbralOficina: 0, umbralOficinaRevocado: -2,
                umbralComedor: 0, umbralComedorRevocado: -3,
                umbralPatio: -1, umbralPatioRevocado: -5,
                umbralBiblioteca: 3, umbralBibliotecaRevocado: 0,
                umbralActividades: 5, umbralActividadesRevocado: 2
            );
            await context.ConfiguracionPrivilegios.AddAsync(config);
        }

        // 2. Semilla de Staff (Principal, Supervisora, Monitora)
        if (!await context.Staff.AnyAsync())
        {
            var principal = new Staff("principal@talentinstitute.com", hasher.Hash("principal123"), Rol.Principal);
            var supervisora = new Staff("supervisora@talentinstitute.com", hasher.Hash("supervisora123"), Rol.Supervisora);
            var monitora = new Staff("monitora@talentinstitute.com", hasher.Hash("monitora123"), Rol.Monitora);

            await context.Staff.AddRangeAsync(principal, supervisora, monitora);
        }

        // 3. Semilla de Paces (Catálogo general)
        if (!await context.Paces.AnyAsync())
        {
            var mat1045 = new Pace("MAT", 1045, 100, 80);
            var mat1046 = new Pace("MAT", 1046, 100, 80);
            var esp1045 = new Pace("ESP", 1045, 100, 80);
            var esp1046 = new Pace("ESP", 1046, 100, 80);

            await context.Paces.AddRangeAsync(mat1045, mat1046, esp1045, esp1046);
            await context.SaveChangesAsync();

            // 4. Semilla de Alumnos
            if (!await context.Alumnos.AnyAsync())
            {
                var mateo = new Alumno("MAT-001", "Mateo", "Fernández", "4 Primaria");
                var sofia = new Alumno("MAT-002", "Sofía", "Martínez", "3 Primaria");

                await context.Alumnos.AddRangeAsync(mateo, sofia);
                await context.SaveChangesAsync();

                // 5. Asignación de PACEs
                var mateoPaceMat = new AlumnoPace(mateo.Id, mat1045.Id, "MAT");
                var sofiaPaceMat = new AlumnoPace(sofia.Id, mat1045.Id, "MAT");

                await context.AlumnoPaces.AddRangeAsync(mateoPaceMat, sofiaPaceMat);
                await context.SaveChangesAsync();

                // Semilla de metas diarias para Mateo
                var metaMateo = new Meta(mateoPaceMat.Id, Turno.Mañana, 5, DateOnly.FromDateTime(DateTime.UtcNow));
                await context.Metas.AddAsync(metaMateo);

                // Activar el PACE de Mateo registrando su primera meta
                mateoPaceMat.RegistrarPrimeraMeta();
                context.AlumnoPaces.Update(mateoPaceMat);

                await context.SaveChangesAsync();
            }
        }
        else
        {
            await context.SaveChangesAsync();
        }
    }
}
