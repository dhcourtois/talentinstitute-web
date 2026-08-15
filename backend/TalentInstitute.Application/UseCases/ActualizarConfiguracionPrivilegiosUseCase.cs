using System;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Application.UseCases;

public class ActualizarConfiguracionPrivilegiosUseCase
{
    private readonly IConfiguracionRepository _configuracionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarConfiguracionPrivilegiosUseCase(
        IConfiguracionRepository configuracionRepository,
        IUnitOfWork unitOfWork)
    {
        _configuracionRepository = configuracionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync(
        int umbralOficina, int umbralOficinaRevocado,
        int umbralComedor, int umbralComedorRevocado,
        int umbralPatio, int umbralPatioRevocado,
        int umbralBiblioteca, int umbralBibliotecaRevocado,
        int umbralActividades, int umbralActividadesRevocado,
        Guid staffIdActualizo,
        CancellationToken cancellationToken = default)
    {
        var configNueva = new ConfiguracionPrivilegios(
            umbralOficina, umbralOficinaRevocado,
            umbralComedor, umbralComedorRevocado,
            umbralPatio, umbralPatioRevocado,
            umbralBiblioteca, umbralBibliotecaRevocado,
            umbralActividades, umbralActividadesRevocado,
            staffIdActualizo
        );

        await _configuracionRepository.AddAsync(configNueva, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
