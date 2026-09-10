using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Domain.Entities;
using TalentInstitute.Domain.Exceptions;

namespace TalentInstitute.Application.UseCases;

public class ObtenerMetasSemanaUseCase
{
    private readonly IPaceRepository _paceRepository;
    private readonly IMetaRepository _metaRepository;

    public ObtenerMetasSemanaUseCase(IPaceRepository paceRepository, IMetaRepository metaRepository)
    {
        _paceRepository = paceRepository;
        _metaRepository = metaRepository;
    }

    public async Task<IReadOnlyList<MetaSemanaDto>> ExecuteAsync(
        Guid alumnoId,
        DateTime fechaInicio,
        CancellationToken cancellationToken = default)
    {
        if (fechaInicio.DayOfWeek != DayOfWeek.Monday)
        {
            throw new DomainException("La fecha de inicio para consultar la semana académica debe ser un lunes.");
        }

        var alumnoPaces = await _paceRepository.GetAlumnoPacesByAlumnoIdAsync(alumnoId, cancellationToken);
        var resultList = new List<MetaSemanaDto>();

        var dateStart = DateOnly.FromDateTime(fechaInicio);
        var dateEnd = DateOnly.FromDateTime(fechaInicio.AddDays(4)); // Lunes a Viernes

        foreach (var ap in alumnoPaces)
        {
            var pace = await _paceRepository.GetByIdAsync(ap.PaceId, cancellationToken);
            var metas = await _metaRepository.GetByAlumnoPaceIdAsync(ap.Id, cancellationToken);

            var metasFiltradas = metas.Where(m => m.FechaObjetivo >= dateStart && m.FechaObjetivo <= dateEnd);

            foreach (var m in metasFiltradas)
            {
                resultList.Add(new MetaSemanaDto
                {
                    Id = m.Id,
                    AlumnoPaceId = m.AlumnoPaceId,
                    Turno = m.Turno.ToString(),
                    PaginaInicial = m.PaginaInicial,
                    PaginaFinal = m.PaginaFinal,
                    PaginasObjetivo = m.PaginasObjetivo,
                    FechaObjetivo = m.FechaObjetivo,
                    PuntajeObtenido = m.PuntajeObtenido,
                    Estado = m.Estado.ToString(),
                    Materia = ap.Materia,
                    NumeroPace = pace?.Numero ?? 0
                });
            }
        }

        return resultList.OrderBy(m => m.FechaObjetivo).ThenBy(m => m.Turno).ToList();
    }
}

public class MetaSemanaDto
{
    public Guid Id { get; set; }
    public Guid AlumnoPaceId { get; set; }
    public string Turno { get; set; } = string.Empty;
    public int PaginaInicial { get; set; }
    public int PaginaFinal { get; set; }

    /// <summary>Derivada del rango; se envía calculada para que la pantalla no la recalcule.</summary>
    public int PaginasObjetivo { get; set; }
    public DateOnly FechaObjetivo { get; set; }
    public decimal? PuntajeObtenido { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string Materia { get; set; } = string.Empty;
    public int NumeroPace { get; set; }
}
