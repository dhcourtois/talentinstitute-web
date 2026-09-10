using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Application.Interfaces;

public interface IAnotacionRepository
{
    Task<IReadOnlyList<Anotacion>> GetByAlumnoIdAsync(Guid alumnoId, CancellationToken cancellationToken = default);
    Task AddAsync(Anotacion anotacion, CancellationToken cancellationToken = default);
}
