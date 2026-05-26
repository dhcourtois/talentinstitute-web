using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Application.Interfaces;

namespace TalentInstitute.Application.UseCases;

public class ObtenerPacesAlumnoUseCase
{
    private readonly IPaceRepository _paceRepository;

    public ObtenerPacesAlumnoUseCase(IPaceRepository paceRepository)
    {
        _paceRepository = paceRepository;
    }

    public async Task<IReadOnlyList<AlumnoPaceDto>> ExecuteAsync(Guid alumnoId, CancellationToken cancellationToken = default)
    {
        var alumnoPaces = await _paceRepository.GetAlumnoPacesByAlumnoIdAsync(alumnoId, cancellationToken);
        var resultList = new List<AlumnoPaceDto>();

        foreach (var ap in alumnoPaces)
        {
            var pace = await _paceRepository.GetByIdAsync(ap.PaceId, cancellationToken);
            resultList.Add(new AlumnoPaceDto
            {
                Id = ap.Id,
                AlumnoId = ap.AlumnoId,
                PaceId = ap.PaceId,
                Estado = ap.Estado.ToString(),
                Materia = ap.Materia,
                FechaInicio = ap.FechaInicio,
                FechaCompletado = ap.FechaCompletado,
                PuntajeFinal = ap.PuntajeFinal,
                NumeroPace = pace?.Numero ?? 0,
                PuntajeMaximo = pace?.PuntajeMaximo ?? 100
            });
        }

        return resultList;
    }
}

public class AlumnoPaceDto
{
    public Guid Id { get; set; }
    public Guid AlumnoId { get; set; }
    public Guid PaceId { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string Materia { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaCompletado { get; set; }
    public decimal? PuntajeFinal { get; set; }
    public int NumeroPace { get; set; }
    public int PuntajeMaximo { get; set; }
}
