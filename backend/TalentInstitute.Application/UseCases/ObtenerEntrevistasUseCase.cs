using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Application.UseCases;

public sealed class ObtenerEntrevistasUseCase
{
    private readonly IEntrevistaRepository _repo;

    public ObtenerEntrevistasUseCase(IEntrevistaRepository repo) => _repo = repo;

    public Task<IReadOnlyList<EntrevistaPadre>> ExecuteAsync(CancellationToken cancellationToken = default)
        => _repo.GetAllAsync(cancellationToken);
}
