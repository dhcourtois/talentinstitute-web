using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Domain.Entities;
using TalentInstitute.Domain.Exceptions;

namespace TalentInstitute.Application.UseCases;

public class ActualizarEstatusMetaUseCase
{
    private readonly IMetaRepository _metaRepository;
    private readonly IPaceRepository _paceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarEstatusMetaUseCase(
        IMetaRepository metaRepository,
        IPaceRepository paceRepository,
        IUnitOfWork unitOfWork)
    {
        _metaRepository = metaRepository;
        _paceRepository = paceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync(
        Guid metaId,
        string nuevoEstado,
        decimal? puntajeObtenido = null,
        CancellationToken cancellationToken = default)
    {
        var meta = await _metaRepository.GetByIdAsync(metaId, cancellationToken)
            ?? throw new KeyNotFoundException($"Meta con id '{metaId}' no encontrada.");

        var alumnoPace = await _paceRepository.GetAlumnoPaceByIdAsync(meta.AlumnoPaceId, cancellationToken)
            ?? throw new KeyNotFoundException($"AlumnoPace con id '{meta.AlumnoPaceId}' no encontrado.");

        var pace = await _paceRepository.GetByIdAsync(alumnoPace.PaceId, cancellationToken)
            ?? throw new KeyNotFoundException($"PACE con id '{alumnoPace.PaceId}' no encontrado.");

        if (!Enum.TryParse<EstadoMeta>(nuevoEstado, true, out var estadoEnum))
        {
            throw new DomainException($"El estado '{nuevoEstado}' no es un estado válido de Meta.");
        }

        switch (estadoEnum)
        {
            case EstadoMeta.EnProgreso:
                if (meta.Estado == EstadoMeta.Pendiente)
                {
                    meta.IniciarProgreso();
                }
                else if (meta.Estado == EstadoMeta.Rechazada)
                {
                    meta.RetomarTrasRechazo();
                }
                else
                {
                    throw new DomainException($"No se puede mover la meta a EnProgreso desde '{meta.Estado}'.");
                }
                break;

            case EstadoMeta.Completada:
                if (meta.Estado == EstadoMeta.Pendiente)
                {
                    meta.IniciarProgreso();
                }
                if (meta.Estado == EstadoMeta.EnProgreso)
                {
                    meta.Completar();
                }
                else
                {
                    throw new DomainException($"No se puede completar la meta en el estado actual '{meta.Estado}'.");
                }
                break;

            case EstadoMeta.Rechazada:
                if (meta.Estado == EstadoMeta.Pendiente)
                {
                    meta.IniciarProgreso();
                }
                if (meta.Estado == EstadoMeta.EnProgreso)
                {
                    meta.Rechazar();
                }
                else
                {
                    throw new DomainException($"No se puede rechazar la meta en el estado actual '{meta.Estado}'.");
                }
                break;

            case EstadoMeta.Scored:
                if (puntajeObtenido == null)
                {
                    throw new DomainException("Se debe proporcionar el puntaje obtenido para realizar el Score Station.");
                }

                meta.RegistrarScore(puntajeObtenido.Value, pace.PuntajeMaximo);

                // Si el puntaje obtenido es mayor o igual al mínimo de aprobación del PACE, se aprueba automáticamente.
                if (puntajeObtenido.Value >= pace.PuntajeMinimoAprobacion)
                {
                    meta.Aprobar();

                    // Comprobar si todas las metas de este PACE están aprobadas,
                    // lo cual transiciona el PACE a ListoParaAutoTest
                    var todasMetas = await _metaRepository.GetByAlumnoPaceIdAsync(meta.AlumnoPaceId, cancellationToken);
                    
                    // Incluyendo la actual (que ya fue marcada Aprobada en memoria, pero la lista cargada de base de datos podría no tenerla reflejada en base de datos)
                    bool todasAprobadas = todasMetas
                        .Where(m => m.Id != meta.Id)
                        .All(m => m.Estado == EstadoMeta.Aprobada);

                    if (todasAprobadas)
                    {
                        alumnoPace.MarcarListoParaAutoTest();
                    }
                }
                else
                {
                    meta.RechazarTrasScore();
                }
                break;

            default:
                throw new DomainException($"Transición al estado '{nuevoEstado}' no está permitida mediante esta acción de usuario.");
        }

        await _metaRepository.UpdateAsync(meta, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
