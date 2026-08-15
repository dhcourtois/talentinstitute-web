using System;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Application.UseCases;

public sealed class RegistrarEntrevistaUseCase
{
    private readonly IEntrevistaRepository _repo;
    private readonly IUnitOfWork _uow;

    public RegistrarEntrevistaUseCase(IEntrevistaRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow  = uow;
    }

    public async Task<Guid> ExecuteAsync(
        string nombrePadre,
        int    numeroHijos,
        bool   riesgoViolencia,
        bool   riesgoDivorcio,
        bool   conoceADios,
        string comentarios,
        bool   aceptado,
        CancellationToken cancellationToken = default)
    {
        var entrevista = new EntrevistaPadre(
            nombrePadre,
            numeroHijos,
            riesgoViolencia,
            riesgoDivorcio,
            conoceADios,
            comentarios,
            aceptado);

        await _repo.AddAsync(entrevista, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return entrevista.Id;
    }
}
