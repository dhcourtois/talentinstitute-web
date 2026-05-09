using System;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Application.Interfaces;

public interface IPaceRepository
{
    Task<Pace> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AlumnoPace> GetAlumnoPaceByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Pace pace, CancellationToken cancellationToken = default);
}
