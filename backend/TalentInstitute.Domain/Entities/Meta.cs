using TalentInstitute.Domain.Exceptions;

namespace TalentInstitute.Domain.Entities;

public class Meta
{
    public Guid Id { get; private set; }
    public Guid AlumnoPaceId { get; private set; }
    public Turno Turno { get; private set; }

    /// <summary>Primera página del rango, inclusive.</summary>
    public int PaginaInicial { get; private set; }

    /// <summary>Última página del rango, inclusive. Igual a la inicial en una meta de una sola página.</summary>
    public int PaginaFinal { get; private set; }

    public DateOnly FechaObjetivo { get; private set; }
    public decimal? PuntajeObtenido { get; private set; }
    public EstadoMeta Estado { get; private set; }

    /// <summary>
    /// Cuántas páginas cubre la meta. Se deriva del rango en lugar de guardarse:
    /// si fuera un campo aparte podría contradecir a `PaginaInicial` y
    /// `PaginaFinal`, y entonces habría dos verdades sobre lo mismo.
    /// </summary>
    public int PaginasObjetivo => PaginaFinal - PaginaInicial + 1;

    // Requerido por EF Core.
    private Meta() { }

    /// <param name="paginaFinal">
    /// Inclusive. Igual a <paramref name="paginaInicial"/> registra una sola
    /// página, que es el comportamiento que existía antes del issue #6.
    /// </param>
    public Meta(Guid alumnoPaceId, Turno turno, int paginaInicial, int paginaFinal, DateOnly fechaObjetivo)
    {
        if (paginaInicial <= 0)
            throw new DomainException("La página inicial debe ser mayor a 0.");

        if (paginaFinal < paginaInicial)
            throw new DomainException("La página final no puede ser menor que la página inicial.");

        Id = Guid.NewGuid();
        AlumnoPaceId = alumnoPaceId;
        Turno = turno;
        PaginaInicial = paginaInicial;
        PaginaFinal = paginaFinal;
        FechaObjetivo = fechaObjetivo;
        Estado = EstadoMeta.Pendiente;
    }

    /// <summary>
    /// Valida que el rango quepa en el PACE. Se expone aparte del constructor
    /// porque el total de páginas vive en `Pace` y la meta no lo conoce.
    /// </summary>
    public static void ValidarRangoContraPace(int paginaInicial, int paginaFinal, int? totalPaginasDelPace)
    {
        if (!totalPaginasDelPace.HasValue)
            return; // PACE sin total capturado: no hay contra qué validar.

        if (paginaFinal > totalPaginasDelPace.Value)
        {
            throw new DomainException(
                $"El rango va hasta la página {paginaFinal}, pero el PACE solo tiene {totalPaginasDelPace.Value} páginas.");
        }
    }

    // Pendiente → EnProgreso (automático al inicio del turno)
    public void IniciarProgreso()
    {
        if (Estado != EstadoMeta.Pendiente)
            throw new DomainException($"No se puede iniciar el progreso desde el estado '{Estado}'.");

        Estado = EstadoMeta.EnProgreso;
    }

    // EnProgreso → Completada (Monitora o Supervisora)
    public void Completar()
    {
        if (Estado != EstadoMeta.EnProgreso)
            throw new DomainException($"Solo se puede completar una meta en progreso. Estado actual: '{Estado}'.");

        Estado = EstadoMeta.Completada;
    }

    // EnProgreso → Rechazada (Monitora o Supervisora)
    public void Rechazar()
    {
        if (Estado != EstadoMeta.EnProgreso)
            throw new DomainException($"Solo se puede rechazar una meta en progreso. Estado actual: '{Estado}'.");

        Estado = EstadoMeta.Rechazada;
    }

    // Rechazada → EnProgreso (automático, el alumno retoma)
    public void RetomarTrasRechazo()
    {
        if (Estado != EstadoMeta.Rechazada)
            throw new DomainException("Solo se puede retomar una meta rechazada.");

        Estado = EstadoMeta.EnProgreso;
    }

    // Completada → Scored (Supervisora, registra puntaje)
    public void RegistrarScore(decimal puntajeObtenido, decimal puntajeMaximo)
    {
        if (Estado != EstadoMeta.Completada)
            throw new DomainException($"Solo se puede registrar score a una meta completada. Estado actual: '{Estado}'.");

        if (puntajeObtenido < 0)
            throw new DomainException("El puntaje obtenido no puede ser negativo.");

        if (puntajeObtenido > puntajeMaximo)
            throw new DomainException("El puntaje obtenido no puede exceder el puntaje máximo del PACE.");

        PuntajeObtenido = puntajeObtenido;
        Estado = EstadoMeta.Scored;
    }

    // Scored → Aprobada (automático si puntaje >= mínimo)
    public void Aprobar()
    {
        if (Estado != EstadoMeta.Scored)
            throw new DomainException("Solo se puede aprobar una meta en estado Scored.");

        Estado = EstadoMeta.Aprobada;
    }

    // Scored → Rechazada (Supervisora, puntaje insuficiente)
    public void RechazarTrasScore()
    {
        if (Estado != EstadoMeta.Scored)
            throw new DomainException("Solo se puede rechazar tras score desde el estado Scored.");

        Estado = EstadoMeta.Rechazada;
    }
}
