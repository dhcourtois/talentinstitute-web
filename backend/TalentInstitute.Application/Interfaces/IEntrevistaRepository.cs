using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Application.Interfaces;

public interface IEntrevistaRepository
{
    Task<IReadOnlyList<EntrevistaPadre>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<EntrevistaPadre?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(EntrevistaPadre entrevista, CancellationToken cancellationToken = default);
}
