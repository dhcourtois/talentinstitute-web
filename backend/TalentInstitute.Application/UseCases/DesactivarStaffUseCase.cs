using System;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Application.Interfaces;

namespace TalentInstitute.Application.UseCases;

public class DesactivarStaffUseCase
{
    private readonly IStaffRepository _staffRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DesactivarStaffUseCase(IStaffRepository staffRepository, IUnitOfWork unitOfWork)
    {
        _staffRepository = staffRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync(Guid staffId, CancellationToken cancellationToken = default)
    {
        var staff = await _staffRepository.GetByIdAsync(staffId, cancellationToken)
            ?? throw new KeyNotFoundException($"Miembro de Staff con id '{staffId}' no encontrado.");

        staff.Desactivar();

        await _staffRepository.UpdateAsync(staff, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
