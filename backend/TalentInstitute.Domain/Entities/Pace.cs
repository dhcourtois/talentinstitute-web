using System;
using System.Linq;
using TalentInstitute.Domain.Exceptions;

namespace TalentInstitute.Domain.Entities;

public class Pace
{
    /// <summary>Largo de la columna que guarda el número.</summary>
    public const int LargoMaximoNumero = 20;

    public Guid Id { get; private set; }
    public string Materia { get; private set; } // e.g. MAT

    /// <summary>
    /// Identificador del cuadernillo dentro de la materia. Es alfanumérico, no
    /// entero: además de los cuadernillos numerados (1045, 1046) el colegio
    /// maneja códigos con letras como RR01. Se guarda normalizado en mayúsculas
    /// para que "rr01" y "RR01" sean el mismo PACE.
    /// </summary>
    public string Numero { get; private set; }

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
        Numero = string.Empty;
    }

    public Pace(string materia, string numero, int puntajeMaximo = 100, int puntajeMinimoAprobacion = 80, int? totalPaginas = null)
    {
        if (totalPaginas.HasValue && totalPaginas.Value <= 0)
            throw new DomainException("El total de páginas del PACE debe ser mayor a 0.");

        Id = Guid.NewGuid();
        Materia = materia;
        Numero = NormalizarNumero(numero);
        PuntajeMaximo = puntajeMaximo;
        PuntajeMinimoAprobacion = puntajeMinimoAprobacion;
        TotalPaginas = totalPaginas;
    }

    /// <summary>
    /// Deja el número en su forma canónica: sin espacios alrededor y en
    /// mayúsculas.
    ///
    /// Vive en el dominio y no en el caso de uso a propósito. La normalización
    /// es lo que hace que la comprobación de duplicados funcione, así que
    /// dejarla fuera permitiría crear el mismo PACE dos veces por una ruta que
    /// olvidara aplicarla.
    /// </summary>
    public static string NormalizarNumero(string numero)
    {
        if (string.IsNullOrWhiteSpace(numero))
            throw new DomainException("El número de PACE no puede estar vacío.");

        var limpio = numero.Trim().ToUpperInvariant();

        if (limpio.Length > LargoMaximoNumero)
            throw new DomainException($"El número de PACE no puede exceder {LargoMaximoNumero} caracteres.");

        if (!limpio.All(char.IsLetterOrDigit))
            throw new DomainException("El número de PACE solo admite letras y dígitos, sin espacios ni signos.");

        return limpio;
    }
}
