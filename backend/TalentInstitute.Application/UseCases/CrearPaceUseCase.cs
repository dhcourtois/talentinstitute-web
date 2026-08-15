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
        int numeroPace,
        int puntajeMaximo,
        int puntajeMinimoAprobacion,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(materia))
        {
            throw new DomainException("La materia del PACE no puede estar vacía.");
        }

        if (numeroPace <= 0)
        {
            throw new DomainException("El número de PACE debe ser mayor a 0.");
        }

        if (puntajeMaximo <= 0)
        {
            throw new DomainException("El puntaje máximo del PACE debe ser mayor a 0.");
        }

        if (puntajeMinimoAprobacion < 0 || puntajeMinimoAprobacion > puntajeMaximo)
        {
            throw new DomainException("El puntaje mínimo de aprobación debe estar entre 0 y el puntaje máximo.");
        }

        var materiaNormalizada = materia.Trim().ToUpperInvariant();
        var existente = await _paceRepository.GetByMateriaYNumeroAsync(materiaNormalizada, numeroPace, cancellationToken);

        if (existente is not null)
        {
            throw new DomainException($"Ya existe el PACE {materiaNormalizada}-{numeroPace} en el catálogo.");
        }

        var pace = new Pace(materiaNormalizada, numeroPace, puntajeMaximo, puntajeMinimoAprobacion);

        await _paceRepository.AddAsync(pace, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return pace.Id;
    }
}
