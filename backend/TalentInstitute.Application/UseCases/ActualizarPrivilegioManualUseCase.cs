using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Application.UseCases;

/// <summary>
/// Fuerza un privilegio de un alumno a activo o inactivo, o lo devuelve a
/// automático (issue #21).
/// </summary>
public class ActualizarPrivilegioManualUseCase
{
    private readonly IAlumnoRepository _alumnoRepository;
    private readonly IConfiguracionRepository _configuracionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarPrivilegioManualUseCase(
        IAlumnoRepository alumnoRepository,
        IConfiguracionRepository configuracionRepository,
        IUnitOfWork unitOfWork)
    {
        _alumnoRepository = alumnoRepository;
        _configuracionRepository = configuracionRepository;
        _unitOfWork = unitOfWork;
    }

    /// <param name="activo">
    /// Nulo devuelve el privilegio a automático: a partir de ahí vuelve a
    /// mandar el balance de méritos.
    /// </param>
    public async Task ExecuteAsync(
        Guid alumnoId,
        Privilegio privilegio,
        bool? activo,
        CancellationToken cancellationToken = default)
    {
        var alumno = await _alumnoRepository.GetByIdAsync(alumnoId, cancellationToken)
            ?? throw new KeyNotFoundException($"Alumno con id '{alumnoId}' no encontrado.");

        // Los umbrales importan incluso al forzar: al volver a automático, el
        // privilegio tiene que reevaluarse contra la configuración vigente.
        var config = await _configuracionRepository.GetActivaAsync(cancellationToken)
            ?? new ConfiguracionPrivilegios();

        alumno.EstablecerPrivilegioManual(privilegio, activo, config);

        await _alumnoRepository.UpdateAsync(alumno, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
