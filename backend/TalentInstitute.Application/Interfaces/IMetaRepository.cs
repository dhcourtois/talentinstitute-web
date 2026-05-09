using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Application.Interfaces;

public interface IMetaRepository
{
    Task<Meta?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Meta>> GetByAlumnoPaceIdAsync(Guid alumnoPaceId, CancellationToken cancellationToken = default);
    Task AddAsync(Meta meta, CancellationToken cancellationToken = default);
    Task UpdateAsync(Meta meta, CancellationToken cancellationToken = default);
}
