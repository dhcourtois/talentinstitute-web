using Microsoft.EntityFrameworkCore;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Domain.Entities;
using TalentInstitute.Infrastructure.Data;

namespace TalentInstitute.Infrastructure.Repositories;

public class StaffRepository : IStaffRepository
{
    private readonly TalentInstituteDbContext _context;

    public StaffRepository(TalentInstituteDbContext context) => _context = context;

    public async Task<Staff?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _context.Staff.FirstOrDefaultAsync(s => s.Email == email.ToLowerInvariant(), cancellationToken);

    public async Task<Staff?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Staff.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<Staff>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Staff.ToListAsync(cancellationToken);

    public async Task AddAsync(Staff staff, CancellationToken cancellationToken = default)
        => await _context.Staff.AddAsync(staff, cancellationToken);

    public Task UpdateAsync(Staff staff, CancellationToken cancellationToken = default)
    {
        _context.Staff.Update(staff);
        return Task.CompletedTask;
    }
}
