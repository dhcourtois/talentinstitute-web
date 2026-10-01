using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Domain.Entities;
using TalentInstitute.Domain.Exceptions;

namespace TalentInstitute.Application.UseCases;

/// <summary>
/// Hace avanzar un PACE asignado por el flujo ACE.
///
/// El dominio siempre tuvo las cuatro transiciones, pero solo el auto-test
/// estaba expuesto en la API. Sin las otras tres el PACE se quedaba en
/// `EnProgreso` de forma permanente y, como un PACE activo bloquea su materia,
/// el alumno tampoco podía recibir uno nuevo. Este caso de uso cierra el flujo.
/// </summary>
public class AvanzarEstadoPaceUseCase
{
    private readonly IPaceRepository _paceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AvanzarEstadoPaceUseCase(IPaceRepository paceRepository, IUnitOfWork unitOfWork)
    {
        _paceRepository = paceRepository;
        _unitOfWork = unitOfWork;
    }

    /// <param name="exitoso">
    /// Requerido por `RegistrarAutoTest` y `EvaluarTestFinal`; se ignora en las
    /// transiciones que no tienen dos salidas posibles.
    /// </param>
    /// <param name="puntajeFinal">Opcional, solo al evaluar el test final.</param>
    public async Task<PaceEstado> ExecuteAsync(
        Guid alumnoPaceId,
        AccionPace accion,
        bool? exitoso = null,
        decimal? puntajeFinal = null,
        CancellationToken cancellationToken = default)
    {
        var alumnoPace = await _paceRepository.GetAlumnoPaceByIdAsync(alumnoPaceId, cancellationToken)
            ?? throw new KeyNotFoundException($"PACE asignado con id '{alumnoPaceId}' no encontrado.");

        switch (accion)
        {
            case AccionPace.MarcarListoParaAutoTest:
                alumnoPace.MarcarListoParaAutoTest();
                break;

            case AccionPace.RegistrarAutoTest:
                alumnoPace.CompletarAutoTest(RequerirResultado(exitoso, "el auto-test"));
                break;

            case AccionPace.ProgramarTestFinal:
                alumnoPace.ProgramarTestFinal();
                break;

            case AccionPace.EvaluarTestFinal:
                alumnoPace.EvaluarTestFinal(RequerirResultado(exitoso, "el test final"), puntajeFinal);
                break;

            default:
                throw new DomainException($"La acción '{accion}' no es válida.");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return alumnoPace.Estado;
    }

    /// <summary>
    /// Un resultado ausente no puede asumirse como aprobado: cerraría un PACE
    /// como completado sin que nadie lo haya dicho.
    /// </summary>
    private static bool RequerirResultado(bool? exitoso, string paso)
        => exitoso ?? throw new DomainException($"Hay que indicar si {paso} fue aprobado o no.");
}
