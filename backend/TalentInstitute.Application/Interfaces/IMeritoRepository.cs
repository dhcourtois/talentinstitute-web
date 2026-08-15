using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Application.Interfaces;

public interface IMeritoRepository
{
    Task<Merito?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Merito>> GetByAlumnoIdAsync(Guid alumnoId, CancellationToken cancellationToken = default);
    Task AddAsync(Merito merito, CancellationToken cancellationToken = default);
    Task UpdateAsync(Merito merito, CancellationToken cancellationToken = default);
}
