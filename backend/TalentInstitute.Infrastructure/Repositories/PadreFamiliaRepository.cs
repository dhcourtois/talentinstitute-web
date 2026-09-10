using Microsoft.EntityFrameworkCore;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Domain.Entities;
using TalentInstitute.Infrastructure.Data;

namespace TalentInstitute.Infrastructure.Repositories;

public class PadreFamiliaRepository : IPadreFamiliaRepository
{
    private readonly TalentInstituteDbContext _context;

    public PadreFamiliaRepository(TalentInstituteDbContext context) => _context = context;

    public async Task<PadreFamilia?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.PadresFamilia.FindAsync([id], cancellationToken);

    public async Task<PadreFamilia?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizado = email.Trim().ToLowerInvariant();
        return await _context.PadresFamilia
            .FirstOrDefaultAsync(p => p.Email == normalizado, cancellationToken);
    }

    public async Task<IReadOnlyList<PadreFamilia>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.PadresFamilia
            .OrderBy(p => p.Nombre)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(PadreFamilia padre, CancellationToken cancellationToken = default)
        => await _context.PadresFamilia.AddAsync(padre, cancellationToken);

    public Task UpdateAsync(PadreFamilia padre, CancellationToken cancellationToken = default)
    {
        _context.PadresFamilia.Update(padre);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<Guid>> GetAlumnoIdsAsync(Guid padreFamiliaId, CancellationToken cancellationToken = default)
        => await _context.PadresAlumnos
            .Where(pa => pa.PadreFamiliaId == padreFamiliaId)
            .Select(pa => pa.AlumnoId)
            .ToListAsync(cancellationToken);

    public async Task<bool> TieneAlumnoAsync(Guid padreFamiliaId, Guid alumnoId, CancellationToken cancellationToken = default)
        => await _context.PadresAlumnos
            .AnyAsync(pa => pa.PadreFamiliaId == padreFamiliaId && pa.AlumnoId == alumnoId, cancellationToken);

    public async Task<IReadOnlyList<PadreAlumno>> GetVinculosAsync(Guid padreFamiliaId, CancellationToken cancellationToken = default)
        => await _context.PadresAlumnos
            .Where(pa => pa.PadreFamiliaId == padreFamiliaId)
            .ToListAsync(cancellationToken);

    public async Task AddVinculoAsync(PadreAlumno vinculo, CancellationToken cancellationToken = default)
        => await _context.PadresAlumnos.AddAsync(vinculo, cancellationToken);

    public Task RemoveVinculoAsync(PadreAlumno vinculo, CancellationToken cancellationToken = default)
    {
        _context.PadresAlumnos.Remove(vinculo);
        return Task.CompletedTask;
    }
}
