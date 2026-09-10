using System;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Domain.Entities;
using TalentInstitute.Domain.Exceptions;

namespace TalentInstitute.Application.UseCases;

public class CrearPaceUseCase
{
    private readonly IPaceRepository _paceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CrearPaceUseCase(IPaceRepository paceRepository, IUnitOfWork unitOfWork)
    {
        _paceRepository = paceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> ExecuteAsync(
        string materia,
        string numeroPace,
        int puntajeMaximo,
        int puntajeMinimoAprobacion,
        int? totalPaginas = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(materia))
        {
            throw new DomainException("La materia del PACE no puede estar vacía.");
        }

        // El formato del número lo valida y normaliza el constructor de `Pace`.
        // Aquí solo se necesita la forma canónica para detectar el duplicado.
        var numeroNormalizado = Pace.NormalizarNumero(numeroPace);

        if (puntajeMaximo <= 0)
        {
            throw new DomainException("El puntaje máximo del PACE debe ser mayor a 0.");
        }

        if (puntajeMinimoAprobacion < 0 || puntajeMinimoAprobacion > puntajeMaximo)
        {
            throw new DomainException("El puntaje mínimo de aprobación debe estar entre 0 y el puntaje máximo.");
        }

        var materiaNormalizada = materia.Trim().ToUpperInvariant();
        var existente = await _paceRepository.GetByMateriaYNumeroAsync(materiaNormalizada, numeroNormalizado, cancellationToken);

        if (existente is not null)
        {
            throw new DomainException($"Ya existe el PACE {materiaNormalizada}-{numeroNormalizado} en el catálogo.");
        }

        var pace = new Pace(materiaNormalizada, numeroNormalizado, puntajeMaximo, puntajeMinimoAprobacion, totalPaginas);

        await _paceRepository.AddAsync(pace, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return pace.Id;
    }
}
