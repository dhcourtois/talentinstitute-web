using System;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Application.Interfaces;

namespace TalentInstitute.Application.UseCases;

public class CheckStudentPaceProgressUseCase
{
    private readonly IPaceRepository _paceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CheckStudentPaceProgressUseCase(IPaceRepository paceRepository, IUnitOfWork unitOfWork)
    {
        _paceRepository = paceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync(Guid alumnoPaceId, bool exitoso, CancellationToken cancellationToken = default)
    {
        var alumnoPace = await _paceRepository.GetAlumnoPaceByIdAsync(alumnoPaceId, cancellationToken);
        if (alumnoPace == null)
            throw new Exception("AlumnoPace no encontrado.");

        // Anotar el estatus de Score (AutoTest)
        alumnoPace.CompletarAutoTest(exitoso);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
