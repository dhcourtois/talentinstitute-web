using TalentInstitute.Domain.Exceptions;

namespace TalentInstitute.Domain.Entities;

public class AlumnoPace
{
    public Guid Id { get; private set; }
    public Guid AlumnoId { get; private set; }
    public Guid PaceId { get; private set; }
    public PaceEstado Estado { get; private set; }
    public string Materia { get; private set; }
    public DateTime FechaInicio { get; private set; }
    public DateTime? FechaCompletado { get; private set; }
    public decimal? PuntajeFinal { get; private set; }
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public AlumnoPace(Guid alumnoId, Guid paceId, string materia)
    {
        Id = Guid.NewGuid();
        AlumnoId = alumnoId;
        PaceId = paceId;
        Materia = materia;
        Estado = PaceEstado.Asignado;
        FechaInicio = DateTime.UtcNow;
    }

    /// <summary>
    /// Valida que no exista ya un AlumnoPace activo para la misma materia.
    /// Debe llamarse antes de persistir el nuevo AlumnoPace.
    /// </summary>
    public static void ValidarAsignacionUnica(IEnumerable<AlumnoPace> pacesActivos, string materia)
    {
        var estadosActivos = new[]
        {
            PaceEstado.Asignado, PaceEstado.EnProgreso, PaceEstado.ListoParaAutoTest,
            PaceEstado.AutoTestOk, PaceEstado.EnTestFinal
        };

        bool existeActivo = pacesActivos.Any(p =>
            p.Materia == materia && estadosActivos.Contains(p.Estado));

        if (existeActivo)
            throw new DomainException($"Ya existe un PACE activo para la materia '{materia}'. No se puede asignar otro hasta que el actual esté Completado o Fallido.");
    }

    public void RegistrarPrimeraMeta()
    {
        if (Estado == PaceEstado.Asignado)
        {
            Estado = PaceEstado.EnProgreso;
        }
    }

    public void CompletarAutoTest(bool exitoso)
    {
        if (Estado != PaceEstado.ListoParaAutoTest && Estado != PaceEstado.AutoTestFallido)
            throw new DomainException("El PACE no está en estado válido para realizar el auto-test.");

        Estado = exitoso ? PaceEstado.AutoTestOk : PaceEstado.AutoTestFallido;
    }

    public void ProgramarTestFinal()
    {
        if (Estado != PaceEstado.AutoTestOk)
            throw new DomainException("El alumno debe haber completado exitosamente el auto-test primero.");

        Estado = PaceEstado.EnTestFinal;
    }

    public void EvaluarTestFinal(bool aprobado)
    {
        if (Estado != PaceEstado.EnTestFinal)
            throw new DomainException("El PACE no está en Test Final.");

        Estado = aprobado ? PaceEstado.Completado : PaceEstado.Fallido;
    }
}
