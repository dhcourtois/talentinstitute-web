using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Domain.Entities;
using TalentInstitute.Infrastructure.Data;

namespace TalentInstitute.Infrastructure.Repositories;

public sealed class EntrevistaRepository : IEntrevistaRepository
{
    private readonly TalentInstituteDbContext _context;

    public EntrevistaRepository(TalentInstituteDbContext context) => _context = context;

    public async Task<IReadOnlyList<EntrevistaPadre>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.EntrevistasPadres
            .OrderByDescending(e => e.FechaEntrevista)
            .ToListAsync(cancellationToken);

    public async Task<EntrevistaPadre?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.EntrevistasPadres.FindAsync([id], cancellationToken);

    public async Task AddAsync(EntrevistaPadre entrevista, CancellationToken cancellationToken = default)
        => await _context.EntrevistasPadres.AddAsync(entrevista, cancellationToken);
}
