using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Application.UseCases;

/// <summary>Registra una observación semanal sobre un alumno (issue #20).</summary>
public class RegistrarAnotacionUseCase
{
    private readonly IAnotacionRepository _anotacionRepository;
    private readonly IAlumnoRepository _alumnoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegistrarAnotacionUseCase(
        IAnotacionRepository anotacionRepository,
        IAlumnoRepository alumnoRepository,
        IUnitOfWork unitOfWork)
    {
        _anotacionRepository = anotacionRepository;
        _alumnoRepository = alumnoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> ExecuteAsync(
        Guid alumnoId,
        Guid staffId,
        string texto,
        DateOnly? fecha = null,
        CancellationToken cancellationToken = default)
    {
        // Se valida la existencia del alumno antes de escribir: la FK lo impediría
        // igual, pero como error de base de datos y no como un 404 legible.
        _ = await _alumnoRepository.GetByIdAsync(alumnoId, cancellationToken)
            ?? throw new KeyNotFoundException($"Alumno con id '{alumnoId}' no encontrado.");

        var anotacion = new Anotacion(alumnoId, staffId, texto, fecha);

        await _anotacionRepository.AddAsync(anotacion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return anotacion.Id;
    }
}
