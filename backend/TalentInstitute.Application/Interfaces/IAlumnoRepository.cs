using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Application.Interfaces;

public interface IAlumnoRepository
{
    Task<Alumno> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Alumno alumno, CancellationToken cancellationToken = default);
    Task UpdateAsync(Alumno alumno, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Alumno>> GetAllAsync(CancellationToken cancellationToken = default);
}
