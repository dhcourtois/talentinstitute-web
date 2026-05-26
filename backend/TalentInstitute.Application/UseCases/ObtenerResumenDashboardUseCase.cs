using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Application.UseCases;

public class ObtenerResumenDashboardUseCase
{
    private readonly IAlumnoRepository _alumnoRepository;
    private readonly IPaceRepository _paceRepository;
    private readonly IMetaRepository _metaRepository;

    public ObtenerResumenDashboardUseCase(
        IAlumnoRepository alumnoRepository,
        IPaceRepository paceRepository,
        IMetaRepository metaRepository)
    {
        _alumnoRepository = alumnoRepository;
        _paceRepository = paceRepository;
        _metaRepository = metaRepository;
    }

    public async Task<DashboardResumenDto> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var alumnos = await _alumnoRepository.GetAllAsync(cancellationToken);
        var alumnoPaces = await _paceRepository.GetAllAlumnoPacesAsync(cancellationToken);
        var metas = await _metaRepository.GetAllAsync(cancellationToken);

        int totalAlumnos = alumnos.Count;

        // Metas completadas hoy (Completada, Scored, Aprobada)
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var completedStates = new[] { EstadoMeta.Completada, EstadoMeta.Scored, EstadoMeta.Aprobada };
        int metasCompletadasHoy = metas.Count(m =>
            m.FechaObjetivo == today &&
            completedStates.Contains(m.Estado));

        // PACEs en revisión (ListoParaAutoTest, AutoTestOk, EnTestFinal)
        var revisionStates = new[] { PaceEstado.ListoParaAutoTest, PaceEstado.AutoTestOk, PaceEstado.EnTestFinal };
        int pacesEnRevision = alumnoPaces.Count(ap => revisionStates.Contains(ap.Estado));

        // Calcular alertas activas (alumnos sin metas completadas en 2+ días hábiles)
        int alertasActivas = 0;
        var oldestWorkingDay = GetOldestWorkingDayOfLastTwo(today);

        var activeStates = new[]
        {
            PaceEstado.Asignado, PaceEstado.EnProgreso, PaceEstado.ListoParaAutoTest,
            PaceEstado.AutoTestOk, PaceEstado.EnTestFinal
        };

        foreach (var alumno in alumnos)
        {
            // Un alumno está activo si tiene al menos un PACE activo asignado
            bool tienePaceActivo = alumnoPaces.Any(ap => ap.AlumnoId == alumno.Id && activeStates.Contains(ap.Estado));
            if (!tienePaceActivo) continue;

            // Obtener todas las metas del alumno
            var pacesAlumnoIds = alumnoPaces.Where(ap => ap.AlumnoId == alumno.Id).Select(ap => ap.Id).ToList();
            var metasAlumno = metas.Where(m => pacesAlumnoIds.Contains(m.AlumnoPaceId)).ToList();

            // Verificar si tiene alguna meta completada en los últimos 2 días hábiles o hoy
            bool tieneMetaCompletadaReciente = metasAlumno.Any(m =>
                m.FechaObjetivo >= oldestWorkingDay &&
                completedStates.Contains(m.Estado));

            if (!tieneMetaCompletadaReciente)
            {
                alertasActivas++;
            }
        }

        return new DashboardResumenDto
        {
            AlumnosActivos = totalAlumnos,
            MetasCompletadasHoy = metasCompletadasHoy,
            PacesEnRevision = pacesEnRevision,
            AlertasActivas = alertasActivas
        };
    }

    public static DateOnly GetOldestWorkingDayOfLastTwo(DateOnly today)
    {
        var workingDays = new List<DateOnly>();
        var current = today;

        while (workingDays.Count < 2)
        {
            current = current.AddDays(-1);
            if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
            {
                workingDays.Add(current);
            }
        }

        return workingDays.Last(); // El más antiguo de los últimos 2 días hábiles
    }
}

public class DashboardResumenDto
{
    public int AlumnosActivos { get; set; }
    public int MetasCompletadasHoy { get; set; }
    public int PacesEnRevision { get; set; }
    public int AlertasActivas { get; set; }
}
