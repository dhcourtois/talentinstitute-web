using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Application.Interfaces;

namespace TalentInstitute.Application.UseCases;

public class ObtenerCatalogoPacesUseCase
{
    private readonly IPaceRepository _paceRepository;

    public ObtenerCatalogoPacesUseCase(IPaceRepository paceRepository)
    {
        _paceRepository = paceRepository;
    }

    public async Task<IReadOnlyList<PaceDto>> ExecuteAsync(string? materia = null, CancellationToken cancellationToken = default)
    {
        var materiaNormalizada = string.IsNullOrWhiteSpace(materia)
            ? null
            : materia.Trim().ToUpperInvariant();

        var paces = await _paceRepository.GetCatalogoAsync(materiaNormalizada, cancellationToken);

        return paces.Select(p => new PaceDto
        {
            Id = p.Id,
            Materia = p.Materia,
            NumeroPace = p.Numero,
            PuntajeMaximo = p.PuntajeMaximo,
            PuntajeMinimoAprobacion = p.PuntajeMinimoAprobacion,
            TotalPaginas = p.TotalPaginas
        }).ToList();
    }
}

public class PaceDto
{
    public Guid Id { get; set; }
    public string Materia { get; set; } = string.Empty;
    public string NumeroPace { get; set; } = string.Empty;
    public int PuntajeMaximo { get; set; }
    public int PuntajeMinimoAprobacion { get; set; }

    /// <summary>Nulo en los PACEs capturados antes del issue #6.</summary>
    public int? TotalPaginas { get; set; }
}
