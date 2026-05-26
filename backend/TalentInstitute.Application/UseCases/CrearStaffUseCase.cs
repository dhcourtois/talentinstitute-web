using System;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Domain.Entities;
using TalentInstitute.Domain.Exceptions;

namespace TalentInstitute.Application.UseCases;

public class CrearStaffUseCase
{
    private readonly IStaffRepository _staffRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public CrearStaffUseCase(
        IStaffRepository staffRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _staffRepository = staffRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> ExecuteAsync(
        string email,
        string password,
        string rol,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            throw new DomainException("La contraseña debe tener al menos 6 caracteres.");
        }

        var existingStaff = await _staffRepository.GetByEmailAsync(email, cancellationToken);
        if (existingStaff != null)
        {
            throw new DomainException($"Ya existe un usuario de personal registrado con el correo '{email}'.");
        }

        if (!Enum.TryParse<Rol>(rol, true, out var rolEnum))
        {
            throw new DomainException($"El rol '{rol}' no es un rol escolar válido.");
        }

        string passwordHash = _passwordHasher.Hash(password);
        var staff = new Staff(email, passwordHash, rolEnum);

        await _staffRepository.AddAsync(staff, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return staff.Id;
    }
}
