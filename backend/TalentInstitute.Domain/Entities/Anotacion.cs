using TalentInstitute.Domain.Exceptions;

namespace TalentInstitute.Domain.Entities;

/// <summary>
/// Observación sobre un alumno, anclada a una semana (issue #20).
///
/// El reporte semanal es la unidad de trabajo del colegio, así que la anotación
/// guarda el lunes de su semana como dato propio en vez de deducirlo al leer:
/// el agrupado y el filtrado por semana se resuelven en la base y no cambian si
/// alguien consulta el historial meses después.
/// </summary>
public class Anotacion
{
    /// <summary>Tope de la columna. Es una observación semanal, no un expediente.</summary>
    public const int LargoMaximo = 2000;

    public Guid Id { get; private set; }
    public Guid AlumnoId { get; private set; }
    public Guid StaffId { get; private set; }

    /// <summary>Lunes de la semana a la que pertenece la observación.</summary>
    public DateOnly SemanaInicio { get; private set; }

    public string Texto { get; private set; }
    public DateTime FechaCreacion { get; private set; }

    // Requerido por EF Core.
    private Anotacion()
    {
        Texto = string.Empty;
    }

    /// <param name="fecha">
    /// Día al que corresponde la observación; se ancla al lunes de esa semana.
    /// Omitirlo la registra en la semana en curso.
    /// </param>
    public Anotacion(Guid alumnoId, Guid staffId, string texto, DateOnly? fecha = null)
    {
        if (alumnoId == Guid.Empty)
            throw new DomainException("La anotación debe pertenecer a un alumno.");

        if (staffId == Guid.Empty)
            throw new DomainException("La anotación debe tener un autor.");

        if (string.IsNullOrWhiteSpace(texto))
            throw new DomainException("El texto de la anotación no puede estar vacío.");

        var limpio = texto.Trim();
        if (limpio.Length > LargoMaximo)
            throw new DomainException($"El texto de la anotación no puede exceder {LargoMaximo} caracteres.");

        Id = Guid.NewGuid();
        AlumnoId = alumnoId;
        StaffId = staffId;
        Texto = limpio;
        FechaCreacion = DateTime.UtcNow;
        SemanaInicio = InicioDeSemana(fecha ?? DateOnly.FromDateTime(DateTime.UtcNow));
    }

    /// <summary>
    /// Lunes de la semana de <paramref name="fecha"/>. La semana académica corre
    /// de lunes a domingo, igual que en el módulo de metas.
    /// </summary>
    public static DateOnly InicioDeSemana(DateOnly fecha)
    {
        // DayOfWeek numera desde el domingo; el desplazamiento mueve el origen al lunes.
        int desplazamiento = ((int)fecha.DayOfWeek + 6) % 7;
        return fecha.AddDays(-desplazamiento);
    }
}
