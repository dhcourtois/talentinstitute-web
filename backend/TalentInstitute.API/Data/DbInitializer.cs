using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

    /// <summary>
    /// Crea la cuenta Principal inicial a partir de configuración, y solo si la
    /// tabla Staff está vacía.
    ///
    /// SeedAsync corre únicamente en Development, así que en Azure la base
    /// arranca sin ningún usuario tras un despliegue limpio. Como StaffController
    /// exige [Authorize(Roles = "Principal")], no había forma de crear el primer
    /// usuario: nadie podía entrar y nadie podía darse de alta.
    ///
    /// Se configura con Bootstrap__AdminEmail y Bootstrap__AdminPassword. Si no
    /// están definidas no hace nada, así que no cambia el comportamiento actual
    /// hasta que se definan. Conviene borrarlas una vez creada la cuenta.
    /// </summary>
    public static async Task BootstrapAdminAsync(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DbInitializer));
        var context = scope.ServiceProvider.GetRequiredService<TalentInstituteDbContext>();

        // Nunca toca una base que ya tiene usuarios.
        if (await context.Staff.AnyAsync())
            return;

        var email = configuration["Bootstrap:AdminEmail"];
        var password = configuration["Bootstrap:AdminPassword"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning(
                "La tabla Staff está vacía y no hay Bootstrap:AdminEmail / Bootstrap:AdminPassword configurados. " +
                "Nadie puede iniciar sesión ni crear el primer usuario hasta que se definan.");
            return;
        }

        if (password.Length < 6)
        {
            logger.LogError("Bootstrap:AdminPassword debe tener al menos 6 caracteres. No se creó la cuenta inicial.");
            return;
        }

        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        context.Staff.Add(new Staff(email, hasher.Hash(password), Rol.Principal));
        await context.SaveChangesAsync();

        logger.LogWarning(
            "Cuenta Principal inicial creada para {Email}. Cambia la contraseña y elimina " +
            "Bootstrap:AdminEmail / Bootstrap:AdminPassword de la configuración.", email);
    }
}
