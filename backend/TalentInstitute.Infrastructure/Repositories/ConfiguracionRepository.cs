using Microsoft.EntityFrameworkCore;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Domain.Entities;
using TalentInstitute.Infrastructure.Data;

namespace TalentInstitute.Infrastructure.Repositories;

public class ConfiguracionRepository : IConfiguracionRepository
{
    private readonly TalentInstituteDbContext _context;

    public ConfiguracionRepository(TalentInstituteDbContext context) => _context = context;

    public async Task<ConfiguracionPrivilegios?> GetActivaAsync(CancellationToken cancellationToken = default)
        => await _context.ConfiguracionPrivilegios
            .OrderByDescending(c => EF.Property<DateTime>(c, "FechaActualizacion"))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task AddAsync(ConfiguracionPrivilegios config, CancellationToken cancellationToken = default)
        => await _context.ConfiguracionPrivilegios.AddAsync(config, cancellationToken);
}
