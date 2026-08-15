using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Application.Interfaces;

public interface IStaffRepository
{
    Task<Staff?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Staff?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Staff>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Staff staff, CancellationToken cancellationToken = default);
    Task UpdateAsync(Staff staff, CancellationToken cancellationToken = default);
}
