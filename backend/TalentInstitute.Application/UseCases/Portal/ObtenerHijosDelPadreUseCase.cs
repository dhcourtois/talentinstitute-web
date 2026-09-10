using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Application.UseCases.Portal;

/// <summary>Los alumnos vinculados al padre autenticado (issue #8).</summary>
public class ObtenerHijosDelPadreUseCase
{
    private readonly IPadreFamiliaRepository _padreFamiliaRepository;
    private readonly IAlumnoRepository _alumnoRepository;

    public ObtenerHijosDelPadreUseCase(
        IPadreFamiliaRepository padreFamiliaRepository,
        IAlumnoRepository alumnoRepository)
    {
        _padreFamiliaRepository = padreFamiliaRepository;
        _alumnoRepository = alumnoRepository;
    }

    public async Task<IReadOnlyList<HijoDto>> ExecuteAsync(Guid padreFamiliaId, CancellationToken cancellationToken = default)
    {
        var alumnoIds = await _padreFamiliaRepository.GetAlumnoIdsAsync(padreFamiliaId, cancellationToken);
        var hijos = new List<HijoDto>(alumnoIds.Count);

        foreach (var alumnoId in alumnoIds)
        {
            var alumno = await _alumnoRepository.GetByIdAsync(alumnoId, cancellationToken);

            // Un vínculo puede apuntar a un alumno que ya no está; se omite en
            // silencio en lugar de romper toda la pantalla del padre.
            if (alumno is null) continue;

            hijos.Add(new HijoDto
            {
                Id = alumno.Id,
                NumeroMatricula = alumno.NumeroMatricula,
                Nombre = alumno.Nombre,
                Apellido = alumno.Apellido,
                Nivel = alumno.Nivel,
                BalanceMeritos = alumno.BalanceMeritos,
                PrivilegiosActivos = PrivilegiosActivosDe(alumno.PrivilegeStatus)
            });
        }

        return hijos;
    }

    private static IReadOnlyList<string> PrivilegiosActivosDe(PrivilegeStatus estado)
    {
        var activos = new List<string>(5);
        if (estado.Oficina) activos.Add("Oficina");
        if (estado.Comedor) activos.Add("Comedor");
        if (estado.Patio) activos.Add("Patio");
        if (estado.Biblioteca) activos.Add("Biblioteca");
        if (estado.Actividades) activos.Add("Actividades");
        return activos;
    }
}

/// <summary>
/// Vista reducida del alumno para el portal. No expone la fecha de ingreso ni
/// las anulaciones manuales de privilegios: son datos de operación interna.
/// </summary>
public class HijoDto
{
    public Guid Id { get; set; }
    public string NumeroMatricula { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Nivel { get; set; } = string.Empty;
    public int BalanceMeritos { get; set; }
    public IReadOnlyList<string> PrivilegiosActivos { get; set; } = new List<string>();
}
