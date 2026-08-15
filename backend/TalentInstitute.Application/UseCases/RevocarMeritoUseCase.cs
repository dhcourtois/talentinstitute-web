using System;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Domain.Entities;
using TalentInstitute.Domain.Exceptions;

namespace TalentInstitute.Application.UseCases;

public class RevocarMeritoUseCase
{
    private readonly IMeritoRepository _meritoRepository;
    private readonly IAlumnoRepository _alumnoRepository;
    private readonly IConfiguracionRepository _configuracionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RevocarMeritoUseCase(
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

    public async Task ExecuteAsync(Guid meritoId, Guid staffIdRevoco, CancellationToken cancellationToken = default)
    {
        var merito = await _meritoRepository.GetByIdAsync(meritoId, cancellationToken)
            ?? throw new KeyNotFoundException($"Mérito con id '{meritoId}' no encontrado.");

        if (merito.Revocado)
        {
            throw new DomainException("Este mérito ya ha sido revocado.");
        }

        var alumno = await _alumnoRepository.GetByIdAsync(merito.AlumnoId, cancellationToken)
            ?? throw new KeyNotFoundException($"Alumno con id '{merito.AlumnoId}' no encontrado.");

        var config = await _configuracionRepository.GetActivaAsync(cancellationToken)
            ?? new ConfiguracionPrivilegios();

        // Aplicamos la revocación en el mérito
        merito.Revocar(staffIdRevoco);

        // Revertimos el balance del alumno
        int delta = merito.Tipo == TipoMerito.Merito ? merito.Puntos : -merito.Puntos;
        int nuevoBalance = alumno.BalanceMeritos - delta;

        // Recalculamos los privilegios
        alumno.RecalcularPrivilegios(nuevoBalance, config);

        await _meritoRepository.UpdateAsync(merito, cancellationToken);
        await _alumnoRepository.UpdateAsync(alumno, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
