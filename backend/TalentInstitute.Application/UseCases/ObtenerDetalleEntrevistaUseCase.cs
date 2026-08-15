using System;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Application.UseCases;

public sealed class ObtenerDetalleEntrevistaUseCase
{
    private readonly IEntrevistaRepository _repo;

    public ObtenerDetalleEntrevistaUseCase(IEntrevistaRepository repo) => _repo = repo;

    public async Task<EntrevistaPadre> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entrevista = await _repo.GetByIdAsync(id, cancellationToken);

        if (entrevista is null)
            throw new KeyNotFoundException($"Entrevista con Id '{id}' no encontrada.");

        return entrevista;
    }
}
