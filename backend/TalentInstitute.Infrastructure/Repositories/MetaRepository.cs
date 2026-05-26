using Microsoft.EntityFrameworkCore;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Domain.Entities;
using TalentInstitute.Infrastructure.Data;

namespace TalentInstitute.Infrastructure.Repositories;

public class MetaRepository : IMetaRepository
{
    private readonly TalentInstituteDbContext _context;

    public MetaRepository(TalentInstituteDbContext context) => _context = context;

    public async Task<Meta?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Metas.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<Meta>> GetByAlumnoPaceIdAsync(Guid alumnoPaceId, CancellationToken cancellationToken = default)
        => await _context.Metas
            .Where(m => m.AlumnoPaceId == alumnoPaceId)
            .OrderBy(m => m.FechaObjetivo)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Meta meta, CancellationToken cancellationToken = default)
        => await _context.Metas.AddAsync(meta, cancellationToken);

    public Task UpdateAsync(Meta meta, CancellationToken cancellationToken = default)
    {
        _context.Metas.Update(meta);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<Meta>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Metas.ToListAsync(cancellationToken);
    }
}
