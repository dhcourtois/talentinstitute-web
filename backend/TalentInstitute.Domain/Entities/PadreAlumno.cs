using TalentInstitute.Domain.Exceptions;

namespace TalentInstitute.Domain.Entities;

/// <summary>
/// Vínculo entre un padre de familia y un alumno (issue #8).
///
/// Es la única fuente de verdad sobre qué puede ver un padre. Un hijo puede
/// tener a ambos padres con cuenta, y un padre puede tener varios hijos en el
/// colegio, así que la relación es de muchos a muchos.
/// </summary>
public class PadreAlumno
{
    public Guid Id { get; private set; }
    public Guid PadreFamiliaId { get; private set; }
    public Guid AlumnoId { get; private set; }
    public DateTime FechaVinculo { get; private set; }

    // Requerido por EF Core.
    private PadreAlumno() { }

    public PadreAlumno(Guid padreFamiliaId, Guid alumnoId)
    {
        if (padreFamiliaId == Guid.Empty)
            throw new DomainException("El vínculo debe indicar un padre de familia.");

        if (alumnoId == Guid.Empty)
            throw new DomainException("El vínculo debe indicar un alumno.");

        Id = Guid.NewGuid();
        PadreFamiliaId = padreFamiliaId;
        AlumnoId = alumnoId;
        FechaVinculo = DateTime.UtcNow;
    }
}
