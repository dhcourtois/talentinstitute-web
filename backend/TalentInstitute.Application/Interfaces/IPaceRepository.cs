using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Application.Interfaces;

public interface IPaceRepository
{
    Task<Pace> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AlumnoPace> GetAlumnoPaceByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Pace pace, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AlumnoPace>> GetAlumnoPacesByAlumnoIdAsync(Guid alumnoId, CancellationToken cancellationToken = default);
    Task AddAlumnoPaceAsync(AlumnoPace alumnoPace, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AlumnoPace>> GetAllAlumnoPacesAsync(CancellationToken cancellationToken = default);
}
