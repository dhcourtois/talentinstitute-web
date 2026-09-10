using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Application.Interfaces;

public interface IPadreFamiliaRepository
{
    Task<PadreFamilia?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PadreFamilia?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PadreFamilia>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(PadreFamilia padre, CancellationToken cancellationToken = default);
    Task UpdateAsync(PadreFamilia padre, CancellationToken cancellationToken = default);

    /// <summary>Ids de los alumnos vinculados a un padre. Base de todo el portal.</summary>
    Task<IReadOnlyList<Guid>> GetAlumnoIdsAsync(Guid padreFamiliaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verdadero solo si el alumno está vinculado a ese padre. Es la
    /// comprobación que separa a un padre del expediente de un hijo ajeno.
    /// </summary>
    Task<bool> TieneAlumnoAsync(Guid padreFamiliaId, Guid alumnoId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PadreAlumno>> GetVinculosAsync(Guid padreFamiliaId, CancellationToken cancellationToken = default);
    Task AddVinculoAsync(PadreAlumno vinculo, CancellationToken cancellationToken = default);
    Task RemoveVinculoAsync(PadreAlumno vinculo, CancellationToken cancellationToken = default);
}
