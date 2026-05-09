using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Domain.Entities;
using TalentInstitute.Infrastructure.Data;

namespace TalentInstitute.Infrastructure.Repositories;

public class AlumnoRepository : IAlumnoRepository
{
    private readonly TalentInstituteDbContext _context;

    public AlumnoRepository(TalentInstituteDbContext context)
    {
        _context = context;
    }

    public async Task<Alumno> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Alumnos.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task AddAsync(Alumno alumno, CancellationToken cancellationToken = default)
    {
        await _context.Alumnos.AddAsync(alumno, cancellationToken);
    }

    public Task UpdateAsync(Alumno alumno, CancellationToken cancellationToken = default)
    {
        _context.Alumnos.Update(alumno);
        return Task.CompletedTask;
    }
}
