using Microsoft.EntityFrameworkCore;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Domain.Entities;
using TalentInstitute.Infrastructure.Data;

namespace TalentInstitute.Infrastructure.Repositories;

public class AnotacionRepository : IAnotacionRepository
{
    private readonly TalentInstituteDbContext _context;

    public AnotacionRepository(TalentInstituteDbContext context) => _context = context;

    public async Task<IReadOnlyList<Anotacion>> GetByAlumnoIdAsync(Guid alumnoId, CancellationToken cancellationToken = default)
        => await _context.Anotaciones
            .Where(a => a.AlumnoId == alumnoId)
            .OrderByDescending(a => a.SemanaInicio)
            .ThenByDescending(a => a.FechaCreacion)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Anotacion anotacion, CancellationToken cancellationToken = default)
        => await _context.Anotaciones.AddAsync(anotacion, cancellationToken);
}
