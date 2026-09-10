namespace TalentInstitute.Domain.Entities;

/// <summary>
/// Anulación manual de un privilegio, por alumno (issue #21).
///
/// El sistema decide los privilegios a partir del balance de méritos, y esa
/// sigue siendo la regla de fondo. Pero el colegio necesita casos de excepción
/// —un alumno con incapacidad médica que no puede salir al patio, un permiso
/// puntual del Principal— que el umbral no sabe representar.
///
/// `null` significa "automático": manda el balance. `true` y `false` fuerzan el
/// privilegio y lo mantienen así aunque el balance cambie, hasta que alguien
/// devuelva el privilegio a automático.
/// </summary>
public record PrivilegiosManuales
{
    public bool? Oficina { get; init; }
    public bool? Comedor { get; init; }
    public bool? Patio { get; init; }
    public bool? Biblioteca { get; init; }
    public bool? Actividades { get; init; }

    public PrivilegiosManuales(
        bool? oficina = null,
        bool? comedor = null,
        bool? patio = null,
        bool? biblioteca = null,
        bool? actividades = null)
    {
        Oficina = oficina;
        Comedor = comedor;
        Patio = patio;
        Biblioteca = biblioteca;
        Actividades = actividades;
    }

    /// <summary>Devuelve una copia con un solo privilegio cambiado.</summary>
    public PrivilegiosManuales Con(Privilegio privilegio, bool? valor) => privilegio switch
    {
        Privilegio.Oficina => this with { Oficina = valor },
        Privilegio.Comedor => this with { Comedor = valor },
        Privilegio.Patio => this with { Patio = valor },
        Privilegio.Biblioteca => this with { Biblioteca = valor },
        Privilegio.Actividades => this with { Actividades = valor },
        _ => this
    };

    public bool? De(Privilegio privilegio) => privilegio switch
    {
        Privilegio.Oficina => Oficina,
        Privilegio.Comedor => Comedor,
        Privilegio.Patio => Patio,
        Privilegio.Biblioteca => Biblioteca,
        Privilegio.Actividades => Actividades,
        _ => null
    };

    /// <summary>Verdadero si al menos un privilegio está fuera de automático.</summary>
    public bool HayAlguno =>
        Oficina.HasValue || Comedor.HasValue || Patio.HasValue ||
        Biblioteca.HasValue || Actividades.HasValue;
}
