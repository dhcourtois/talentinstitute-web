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

    public async Task<Pace?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Paces.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Pace?> GetByMateriaYNumeroAsync(string materia, string numero, CancellationToken cancellationToken = default)
    {
        // Se normaliza aquí también para que encontrar un PACE no dependa de
        // cómo escribió el número quien llama: "rr01" y "RR01" son el mismo.
        var normalizado = Pace.NormalizarNumero(numero);

        return await _context.Paces
            .FirstOrDefaultAsync(p => p.Materia == materia && p.Numero == normalizado, cancellationToken);
    }

    public async Task<IReadOnlyList<Pace>> GetCatalogoAsync(string? materia = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Paces.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(materia))
        {
            query = query.Where(p => p.Materia == materia);
        }

        return await query
            .OrderBy(p => p.Materia)
            .ThenBy(p => p.Numero)
            .ToListAsync(cancellationToken);
    }

    public async Task<AlumnoPace?> GetAlumnoPaceByIdAsync(Guid id, CancellationToken cancellationToken = default)
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
