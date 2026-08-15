using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Application.Interfaces;

namespace TalentInstitute.Application.UseCases;

public class ObtenerStaffUseCase
{
    private readonly IStaffRepository _staffRepository;

    public ObtenerStaffUseCase(IStaffRepository staffRepository)
    {
        _staffRepository = staffRepository;
    }

    public async Task<IReadOnlyList<StaffDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var staffList = await _staffRepository.GetAllAsync(cancellationToken);

        return staffList.Select(s => new StaffDto
        {
            Id = s.Id,
            Email = s.Email,
            Rol = s.Rol.ToString(),
            Activo = s.Activo
        }).ToList();
    }
}

public class StaffDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public bool Activo { get; set; }
}
