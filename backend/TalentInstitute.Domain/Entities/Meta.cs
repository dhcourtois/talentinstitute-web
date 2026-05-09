using TalentInstitute.Domain.Exceptions;

namespace TalentInstitute.Domain.Entities;

public class Meta
{
    public Guid Id { get; private set; }
    public Guid AlumnoPaceId { get; private set; }
    public Turno Turno { get; private set; }
    public int PaginasObjetivo { get; private set; }
    public DateOnly FechaObjetivo { get; private set; }
    public decimal? PuntajeObtenido { get; private set; }
    public EstadoMeta Estado { get; private set; }

    public Meta(Guid alumnoPaceId, Turno turno, int paginasObjetivo, DateOnly fechaObjetivo)
    {
        if (paginasObjetivo <= 0)
            throw new DomainException("Las páginas objetivo deben ser mayor a 0.");

        Id = Guid.NewGuid();
        AlumnoPaceId = alumnoPaceId;
        Turno = turno;
        PaginasObjetivo = paginasObjetivo;
        FechaObjetivo = fechaObjetivo;
        Estado = EstadoMeta.Pendiente;
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
