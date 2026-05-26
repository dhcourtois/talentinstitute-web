using System;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Application.UseCases;

public class ObtenerConfiguracionPrivilegiosUseCase
{
    private readonly IConfiguracionRepository _configuracionRepository;

    public ObtenerConfiguracionPrivilegiosUseCase(IConfiguracionRepository configuracionRepository)
    {
        _configuracionRepository = configuracionRepository;
    }

    public async Task<ConfiguracionPrivilegios> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var config = await _configuracionRepository.GetActivaAsync(cancellationToken);
        return config ?? new ConfiguracionPrivilegios();
    }
}
