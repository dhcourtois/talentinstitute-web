using System;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Application.UseCases;

public class AsignarPaceUseCase
{
    private readonly IPaceRepository _paceRepository;
    private readonly IAlumnoRepository _alumnoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AsignarPaceUseCase(
        IPaceRepository paceRepository,
        IAlumnoRepository alumnoRepository,
        IUnitOfWork unitOfWork)
    {
        _paceRepository = paceRepository;
        _alumnoRepository = alumnoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> ExecuteAsync(Guid alumnoId, Guid paceId, CancellationToken cancellationToken = default)
    {
        var alumno = await _alumnoRepository.GetByIdAsync(alumnoId, cancellationToken)
            ?? throw new KeyNotFoundException($"Alumno con id '{alumnoId}' no encontrado.");

        var pace = await _paceRepository.GetByIdAsync(paceId, cancellationToken)
            ?? throw new KeyNotFoundException($"PACE con id '{paceId}' no encontrado.");

        var pacesActivos = await _paceRepository.GetAlumnoPacesByAlumnoIdAsync(alumnoId, cancellationToken);

        AlumnoPace.ValidarAsignacionUnica(pacesActivos, pace.Materia);

        var alumnoPace = new AlumnoPace(alumnoId, paceId, pace.Materia);

        await _paceRepository.AddAlumnoPaceAsync(alumnoPace, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return alumnoPace.Id;
    }
}
