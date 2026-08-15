using System;
using TalentInstitute.Domain.Exceptions;

namespace TalentInstitute.Domain.Entities;

public class EntrevistaPadre
{
    public Guid Id { get; private set; }
    public DateTime FechaEntrevista { get; private set; }
    public string NombrePadre { get; private set; }
    public int NumeroHijos { get; private set; }
    public bool RiesgoViolencia { get; private set; }
    public bool RiesgoDivorcio { get; private set; }
    public bool ConoceADios { get; private set; }
    public string Comentarios { get; private set; }
    public bool Aceptado { get; private set; }

    public EntrevistaPadre(
        string nombrePadre, 
        int numeroHijos, 
        bool riesgoViolencia, 
        bool riesgoDivorcio, 
        bool conoceADios, 
        string comentarios, 
        bool aceptado = false)
    {
        if (string.IsNullOrWhiteSpace(nombrePadre))
            throw new DomainException("El nombre del padre no puede estar vacío.");

        if (numeroHijos < 0)
            throw new DomainException("El número de hijos debe ser mayor o igual a 0.");

        if ((riesgoViolencia || riesgoDivorcio) && string.IsNullOrWhiteSpace(comentarios))
            throw new DomainException("Si hay banderas de riesgo de violencia o divorcio crítico, los comentarios no pueden estar vacíos.");

        Id = Guid.NewGuid();
        FechaEntrevista = DateTime.UtcNow;
        NombrePadre = nombrePadre;
        NumeroHijos = numeroHijos;
        RiesgoViolencia = riesgoViolencia;
        RiesgoDivorcio = riesgoDivorcio;
        ConoceADios = conoceADios;
        Comentarios = comentarios;
        Aceptado = aceptado;
    }
}
