using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Application.UseCases;

public class ObtenerAlertasDashboardUseCase
{
    private readonly IAlumnoRepository _alumnoRepository;
    private readonly IPaceRepository _paceRepository;
    private readonly IMetaRepository _metaRepository;

    public ObtenerAlertasDashboardUseCase(
        IAlumnoRepository alumnoRepository,
        IPaceRepository paceRepository,
        IMetaRepository metaRepository)
    {
        _alumnoRepository = alumnoRepository;
        _paceRepository = paceRepository;
        _metaRepository = metaRepository;
    }

    public async Task<IReadOnlyList<AlertaDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var alumnos = await _alumnoRepository.GetAllAsync(cancellationToken);
        var alumnoPaces = await _paceRepository.GetAllAlumnoPacesAsync(cancellationToken);
        var metas = await _metaRepository.GetAllAsync(cancellationToken);

        var resultList = new List<AlertaDto>();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var oldestWorkingDay = ObtenerResumenDashboardUseCase.GetOldestWorkingDayOfLastTwo(today);
        var completedStates = new[] { EstadoMeta.Completada, EstadoMeta.Scored, EstadoMeta.Aprobada };

        var activeStates = new[]
        {
            PaceEstado.Asignado, PaceEstado.EnProgreso, PaceEstado.ListoParaAutoTest,
            PaceEstado.AutoTestOk, PaceEstado.EnTestFinal
        };

        foreach (var alumno in alumnos)
        {
            bool tienePaceActivo = alumnoPaces.Any(ap => ap.AlumnoId == alumno.Id && activeStates.Contains(ap.Estado));
            if (!tienePaceActivo) continue;

            var pacesAlumnoIds = alumnoPaces.Where(ap => ap.AlumnoId == alumno.Id).Select(ap => ap.Id).ToList();
            var metasAlumno = metas.Where(m => pacesAlumnoIds.Contains(m.AlumnoPaceId)).ToList();

            bool tieneMetaCompletadaReciente = metasAlumno.Any(m =>
                m.FechaObjetivo >= oldestWorkingDay &&
                completedStates.Contains(m.Estado));

            if (!tieneMetaCompletadaReciente)
            {
                var ultimaMeta = metasAlumno
                    .Where(m => completedStates.Contains(m.Estado))
                    .OrderByDescending(m => m.FechaObjetivo)
                    .FirstOrDefault();

                string detalleUltima = ultimaMeta != null
                    ? $"{ultimaMeta.FechaObjetivo:dd/MM/yyyy}"
                    : "Ninguna registrada";

                resultList.Add(new AlertaDto
                {
                    AlumnoId = alumno.Id,
                    Nombre = alumno.Nombre,
                    Apellido = alumno.Apellido,
                    NumeroMatricula = alumno.NumeroMatricula,
                    Nivel = alumno.Nivel,
                    UltimaMetaFecha = detalleUltima
                });
            }
        }

        return resultList;
    }
}

public class AlertaDto
{
    public Guid AlumnoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string NumeroMatricula { get; set; } = string.Empty;
    public string Nivel { get; set; } = string.Empty;
    public string UltimaMetaFecha { get; set; } = string.Empty;
}
