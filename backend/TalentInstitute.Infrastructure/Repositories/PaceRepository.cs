using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Domain.Entities;
using TalentInstitute.Infrastructure.Data;

namespace TalentInstitute.Infrastructure.Repositories;

public class PaceRepository : IPaceRepository
{
    private readonly TalentInstituteDbContext _context;

    public PaceRepository(TalentInstituteDbContext context)
    {
        _context = context;
    }

    public async Task<Pace> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Paces.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<AlumnoPace> GetAlumnoPaceByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AlumnoPaces.FirstOrDefaultAsync(ap => ap.Id == id, cancellationToken);
    }

    public async Task AddAsync(Pace pace, CancellationToken cancellationToken = default)
    {
        await _context.Paces.AddAsync(pace, cancellationToken);
    }

    public async Task<IReadOnlyList<AlumnoPace>> GetAlumnoPacesByAlumnoIdAsync(Guid alumnoId, CancellationToken cancellationToken = default)
    {
        return await _context.AlumnoPaces
            .Where(ap => ap.AlumnoId == alumnoId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAlumnoPaceAsync(AlumnoPace alumnoPace, CancellationToken cancellationToken = default)
    {
        await _context.AlumnoPaces.AddAsync(alumnoPace, cancellationToken);
    }

    public async Task<IReadOnlyList<AlumnoPace>> GetAllAlumnoPacesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AlumnoPaces.ToListAsync(cancellationToken);
    }
}
