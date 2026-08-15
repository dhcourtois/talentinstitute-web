using System;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Domain.Entities;
using TalentInstitute.Domain.Exceptions;

namespace TalentInstitute.Application.UseCases;

public class EditarStaffUseCase
{
    private readonly IStaffRepository _staffRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EditarStaffUseCase(IStaffRepository staffRepository, IUnitOfWork unitOfWork)
    {
        _staffRepository = staffRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync(Guid staffId, string nuevoRol, CancellationToken cancellationToken = default)
    {
        var staff = await _staffRepository.GetByIdAsync(staffId, cancellationToken)
            ?? throw new KeyNotFoundException($"Miembro de Staff con id '{staffId}' no encontrado.");

        if (!Enum.TryParse<Rol>(nuevoRol, true, out var rolEnum))
        {
            throw new DomainException($"El rol '{nuevoRol}' no es un rol escolar válido.");
        }

        staff.ActualizarRol(rolEnum);

        await _staffRepository.UpdateAsync(staff, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
