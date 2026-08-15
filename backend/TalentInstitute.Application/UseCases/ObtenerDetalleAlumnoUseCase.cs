using System;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Application.Interfaces;

namespace TalentInstitute.Application.UseCases;

public class ObtenerDetalleAlumnoUseCase
{
    private readonly IAlumnoRepository _alumnoRepository;

    public ObtenerDetalleAlumnoUseCase(IAlumnoRepository alumnoRepository)
    {
        _alumnoRepository = alumnoRepository;
    }

    public async Task<AlumnoDto> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var a = await _alumnoRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Alumno con id '{id}' no encontrado.");

        return new AlumnoDto
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
            }
        };
    }
}
