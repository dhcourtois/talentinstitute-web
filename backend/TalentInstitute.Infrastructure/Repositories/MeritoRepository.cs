using Microsoft.EntityFrameworkCore;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Domain.Entities;
using TalentInstitute.Infrastructure.Data;

namespace TalentInstitute.Infrastructure.Repositories;

public class MeritoRepository : IMeritoRepository
{
    private readonly TalentInstituteDbContext _context;

    public MeritoRepository(TalentInstituteDbContext context) => _context = context;

    public async Task<Merito?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Meritos.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<Merito>> GetByAlumnoIdAsync(Guid alumnoId, CancellationToken cancellationToken = default)
        => await _context.Meritos
            .Where(m => m.AlumnoId == alumnoId)
            .OrderByDescending(m => m.FechaAplicado)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Merito merito, CancellationToken cancellationToken = default)
        => await _context.Meritos.AddAsync(merito, cancellationToken);

    public Task UpdateAsync(Merito merito, CancellationToken cancellationToken = default)
    {
        _context.Meritos.Update(merito);
        return Task.CompletedTask;
    }
}
