using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Application.Interfaces;

public interface IConfiguracionRepository
{
    Task<ConfiguracionPrivilegios?> GetActivaAsync(CancellationToken cancellationToken = default);
    Task AddAsync(ConfiguracionPrivilegios config, CancellationToken cancellationToken = default);
}
