namespace TalentInstitute.Domain.Entities;

/// <summary>
/// Pasos del flujo ACE por los que avanza un PACE asignado.
///
/// El dominio siempre tuvo estas transiciones, pero solo `RegistrarAutoTest`
/// estaba expuesta en la API. Sin las otras tres, un PACE se quedaba en
/// `EnProgreso` para siempre y, como un PACE activo bloquea la materia, el
/// alumno no podía recibir ninguno nuevo de esa materia.
/// </summary>
public enum AccionPace
{
    /// <summary>EnProgreso (o Asignado) → ListoParaAutoTest.</summary>
    MarcarListoParaAutoTest,

    /// <summary>ListoParaAutoTest → AutoTestOk o AutoTestFallido.</summary>
    RegistrarAutoTest,

    /// <summary>AutoTestOk → EnTestFinal.</summary>
    ProgramarTestFinal,

    /// <summary>EnTestFinal → Completado o Fallido.</summary>
    EvaluarTestFinal
}
