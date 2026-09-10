using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Application.Interfaces;

namespace TalentInstitute.Application.UseCases;

public class ObtenerAlumnosUseCase
{
    private readonly IAlumnoRepository _alumnoRepository;

    public ObtenerAlumnosUseCase(IAlumnoRepository alumnoRepository)
    {
        _alumnoRepository = alumnoRepository;
    }

    public async Task<IReadOnlyList<AlumnoDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var alumnos = await _alumnoRepository.GetAllAsync(cancellationToken);

        return alumnos.Select(a => new AlumnoDto
        {
            Id = a.Id,
            NumeroMatricula = a.NumeroMatricula,
            Nombre = a.Nombre,
            Apellido = a.Apellido,
            Nivel = a.Nivel,
            FechaIngreso = a.FechaIngreso,
            BalanceMeritos = a.BalanceMeritos,
            PrivilegeStatus = new PrivilegeStatusDto
            {
                Oficina = a.PrivilegeStatus.Oficina,
                Comedor = a.PrivilegeStatus.Comedor,
                Patio = a.PrivilegeStatus.Patio,
                Biblioteca = a.PrivilegeStatus.Biblioteca,
                Actividades = a.PrivilegeStatus.Actividades
            },
            PrivilegiosManuales = new PrivilegiosManualesDto
            {
                Oficina = a.PrivilegiosManuales.Oficina,
                Comedor = a.PrivilegiosManuales.Comedor,
                Patio = a.PrivilegiosManuales.Patio,
                Biblioteca = a.PrivilegiosManuales.Biblioteca,
                Actividades = a.PrivilegiosManuales.Actividades
            }
        }).ToList();
    }
}

public class AlumnoDto
{
    public Guid Id { get; set; }
    public string NumeroMatricula { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Nivel { get; set; } = string.Empty;
    public DateTime FechaIngreso { get; set; }
    public int BalanceMeritos { get; set; }
    public PrivilegeStatusDto PrivilegeStatus { get; set; } = new();

    /// <summary>
    /// Qué privilegios están forzados a mano y cuáles siguen en automático.
    /// La interfaz lo necesita para distinguir "activo porque le alcanza el
    /// balance" de "activo porque el Principal lo decidió" (issue #21).
    /// </summary>
    public PrivilegiosManualesDto PrivilegiosManuales { get; set; } = new();
}

public class PrivilegeStatusDto
{
    public bool Oficina { get; set; }
    public bool Comedor { get; set; }
    public bool Patio { get; set; }
    public bool Biblioteca { get; set; }
    public bool Actividades { get; set; }
}

/// <summary>Nulo en un privilegio significa que sigue en automático.</summary>
public class PrivilegiosManualesDto
{
    public bool? Oficina { get; set; }
    public bool? Comedor { get; set; }
    public bool? Patio { get; set; }
    public bool? Biblioteca { get; set; }
    public bool? Actividades { get; set; }
}
