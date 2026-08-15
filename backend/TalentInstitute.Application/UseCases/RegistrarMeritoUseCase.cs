using TalentInstitute.Application.Interfaces;
using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Application.UseCases;

public class RegistrarMeritoUseCase
{
    private readonly IMeritoRepository _meritoRepository;
    private readonly IAlumnoRepository _alumnoRepository;
    private readonly IConfiguracionRepository _configuracionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegistrarMeritoUseCase(
        IMeritoRepository meritoRepository,
        IAlumnoRepository alumnoRepository,
        IConfiguracionRepository configuracionRepository,
        IUnitOfWork unitOfWork)
    {
        _meritoRepository = meritoRepository;
        _alumnoRepository = alumnoRepository;
        _configuracionRepository = configuracionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> ExecuteAsync(
        Guid alumnoId,
        Guid staffId,
        TipoMerito tipo,
        int puntos,
        string motivo,
        CancellationToken cancellationToken = default)
    {
        var alumno = await _alumnoRepository.GetByIdAsync(alumnoId, cancellationToken)
            ?? throw new Exception($"Alumno con id '{alumnoId}' no encontrado.");

        var config = await _configuracionRepository.GetActivaAsync(cancellationToken)
            ?? new ConfiguracionPrivilegios();

        var merito = new Merito(alumnoId, staffId, tipo, puntos, motivo);

        int delta = tipo == TipoMerito.Merito ? puntos : -puntos;
        int nuevoBalance = alumno.BalanceMeritos + delta;

        alumno.RecalcularPrivilegios(nuevoBalance, config);

        await _meritoRepository.AddAsync(merito, cancellationToken);
        await _alumnoRepository.UpdateAsync(alumno, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return merito.Id;
    }
}
