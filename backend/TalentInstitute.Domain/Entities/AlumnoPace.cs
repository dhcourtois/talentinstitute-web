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
        bool existeActivo = pacesActivos.Any(p =>
            p.Materia == materia && EstadosActivos.Contains(p.Estado));

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

    public void MarcarListoParaAutoTest()
    {
        // Se admite también desde Asignado: un PACE al que nunca se le registró
        // una meta no debería quedar atrapado sin poder cerrarse nunca.
        if (Estado != PaceEstado.EnProgreso && Estado != PaceEstado.Asignado)
            throw new DomainException($"Un PACE en estado '{Estado}' ya no puede marcarse como listo para el auto-test.");

        Estado = PaceEstado.ListoParaAutoTest;
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

    /// <param name="puntajeFinal">
    /// Puntaje del Score Station, opcional. Se guarda junto al cierre para que
    /// el historial del alumno pueda mostrarlo.
    /// </param>
    public void EvaluarTestFinal(bool aprobado, decimal? puntajeFinal = null)
    {
        if (Estado != PaceEstado.EnTestFinal)
            throw new DomainException("El PACE no está en Test Final.");

        if (puntajeFinal.HasValue && puntajeFinal.Value < 0)
            throw new DomainException("El puntaje final no puede ser negativo.");

        Estado = aprobado ? PaceEstado.Completado : PaceEstado.Fallido;

        // Sin esto el PACE quedaba cerrado pero sin fecha, y el historial de
        // completados no tenía por qué ordenarse ni qué mostrar.
        FechaCompletado = DateTime.UtcNow;
        PuntajeFinal = puntajeFinal;
    }

    /// <summary>
    /// Estados en los que el PACE sigue ocupando la materia del alumno. Mientras
    /// esté en uno de ellos no se le puede asignar otro PACE de la misma materia.
    /// </summary>
    public static readonly PaceEstado[] EstadosActivos =
    [
        PaceEstado.Asignado, PaceEstado.EnProgreso, PaceEstado.ListoParaAutoTest,
        PaceEstado.AutoTestOk, PaceEstado.AutoTestFallido, PaceEstado.EnTestFinal
    ];

    /// <summary>
    /// Siguiente paso posible desde el estado actual, o nulo si el PACE ya está
    /// cerrado. La interfaz la usa para ofrecer una sola acción por vez en lugar
    /// de pedirle al usuario que adivine el orden del flujo ACE.
    /// </summary>
    public AccionPace? SiguienteAccion => Estado switch
    {
        PaceEstado.Asignado        => AccionPace.MarcarListoParaAutoTest,
        PaceEstado.EnProgreso      => AccionPace.MarcarListoParaAutoTest,
        PaceEstado.ListoParaAutoTest => AccionPace.RegistrarAutoTest,
        PaceEstado.AutoTestFallido => AccionPace.RegistrarAutoTest,
        PaceEstado.AutoTestOk      => AccionPace.ProgramarTestFinal,
        PaceEstado.EnTestFinal     => AccionPace.EvaluarTestFinal,
        _ => null
    };
}
