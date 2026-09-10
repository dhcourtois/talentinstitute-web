using System;
using TalentInstitute.Domain.Exceptions;

namespace TalentInstitute.Domain.Entities;

public class Pace
{
    public Guid Id { get; private set; }
    public string Materia { get; private set; } // e.g. MAT
    public int Numero { get; private set; } // e.g. 1097
    public int PuntajeMaximo { get; private set; }
    public int PuntajeMinimoAprobacion { get; private set; }

    /// <summary>
    /// Total de páginas del cuadernillo. Opcional: los PACEs capturados antes
    /// del issue #6 no lo tienen, y sin él simplemente no se valida el rango de
    /// una meta contra el PACE.
    /// </summary>
    public int? TotalPaginas { get; private set; }

    // Requerido por EF Core.
    private Pace()
    {
        Materia = string.Empty;
    }

    public Pace(string materia, int numero, int puntajeMaximo = 100, int puntajeMinimoAprobacion = 80, int? totalPaginas = null)
    {
        if (totalPaginas.HasValue && totalPaginas.Value <= 0)
            throw new DomainException("El total de páginas del PACE debe ser mayor a 0.");

        Id = Guid.NewGuid();
        Materia = materia;
        Numero = numero;
        PuntajeMaximo = puntajeMaximo;
        PuntajeMinimoAprobacion = puntajeMinimoAprobacion;
        TotalPaginas = totalPaginas;
    }
}
