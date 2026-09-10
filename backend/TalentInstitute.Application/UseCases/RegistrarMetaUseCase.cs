using System;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Domain.Entities;
using TalentInstitute.Domain.Exceptions;

namespace TalentInstitute.Application.UseCases;

public class RegistrarMetaUseCase
{
    private readonly IMetaRepository _metaRepository;
    private readonly IPaceRepository _paceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegistrarMetaUseCase(
        IMetaRepository metaRepository,
        IPaceRepository paceRepository,
        IUnitOfWork unitOfWork)
    {
        _metaRepository = metaRepository;
        _paceRepository = paceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> ExecuteAsync(
        Guid alumnoPaceId,
        Turno turno,
        int paginaInicial,
        int paginaFinal,
        DateOnly fechaObjetivo,
        CancellationToken cancellationToken = default)
    {
        var alumnoPace = await _paceRepository.GetAlumnoPaceByIdAsync(alumnoPaceId, cancellationToken)
            ?? throw new KeyNotFoundException($"AlumnoPace con id '{alumnoPaceId}' no encontrado.");

        // El total de páginas vive en el PACE del catálogo, no en la asignación,
        // así que hay que traerlo para poder validar el rango contra él.
        var pace = await _paceRepository.GetByIdAsync(alumnoPace.PaceId, cancellationToken);
        Meta.ValidarRangoContraPace(paginaInicial, paginaFinal, pace?.TotalPaginas);

        var meta = new Meta(alumnoPaceId, turno, paginaInicial, paginaFinal, fechaObjetivo);

        // Si es la primera meta registrada para el PACE, cambia el estado del PACE de Asignado a EnProgreso
        alumnoPace.RegistrarPrimeraMeta();

        await _metaRepository.AddAsync(meta, cancellationToken);
        // El repositorio de Pace puede no tener un método explícito para Update en la interfaz,
        // pero EF Core rastrea las entidades cargadas si están en el mismo DbContext,
        // por lo que al guardar se persistirán los cambios de alumnoPace automáticamente.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return meta.Id;
    }
}
