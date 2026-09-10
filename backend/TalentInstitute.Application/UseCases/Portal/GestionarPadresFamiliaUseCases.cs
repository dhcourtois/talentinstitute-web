using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Domain.Entities;
using TalentInstitute.Domain.Exceptions;

namespace TalentInstitute.Application.UseCases.Portal;

/// <summary>Alta de una cuenta de padre de familia. Solo el Principal (issue #8).</summary>
public class CrearPadreFamiliaUseCase
{
    private readonly IPadreFamiliaRepository _padreFamiliaRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public CrearPadreFamiliaUseCase(
        IPadreFamiliaRepository padreFamiliaRepository,
        IStaffRepository staffRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _padreFamiliaRepository = padreFamiliaRepository;
        _staffRepository = staffRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> ExecuteAsync(
        string email,
        string password,
        string nombre,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            throw new DomainException("La credencial inicial debe tener al menos 8 caracteres.");

        var normalizado = email?.Trim().ToLowerInvariant() ?? string.Empty;

        if (await _padreFamiliaRepository.GetByEmailAsync(normalizado, cancellationToken) is not null)
            throw new DomainException($"Ya existe una cuenta de padre de familia con el correo '{normalizado}'.");

        // El login busca primero en personal y luego en padres, así que un
        // correo repetido entre las dos tablas dejaría inalcanzable la cuenta
        // del padre. Se bloquea aquí, donde todavía se puede explicar.
        if (await _staffRepository.GetByEmailAsync(normalizado, cancellationToken) is not null)
            throw new DomainException($"El correo '{normalizado}' ya pertenece a una cuenta del personal del colegio.");

        var padre = new PadreFamilia(normalizado, _passwordHasher.Hash(password), nombre);

        await _padreFamiliaRepository.AddAsync(padre, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return padre.Id;
    }
}

/// <summary>Vincula o desvincula un alumno a una cuenta de padre (issue #8).</summary>
public class VincularAlumnoAPadreUseCase
{
    private readonly IPadreFamiliaRepository _padreFamiliaRepository;
    private readonly IAlumnoRepository _alumnoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public VincularAlumnoAPadreUseCase(
        IPadreFamiliaRepository padreFamiliaRepository,
        IAlumnoRepository alumnoRepository,
        IUnitOfWork unitOfWork)
    {
        _padreFamiliaRepository = padreFamiliaRepository;
        _alumnoRepository = alumnoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task VincularAsync(Guid padreFamiliaId, Guid alumnoId, CancellationToken cancellationToken = default)
    {
        _ = await _padreFamiliaRepository.GetByIdAsync(padreFamiliaId, cancellationToken)
            ?? throw new KeyNotFoundException($"Padre de familia con id '{padreFamiliaId}' no encontrado.");

        _ = await _alumnoRepository.GetByIdAsync(alumnoId, cancellationToken)
            ?? throw new KeyNotFoundException($"Alumno con id '{alumnoId}' no encontrado.");

        if (await _padreFamiliaRepository.TieneAlumnoAsync(padreFamiliaId, alumnoId, cancellationToken))
            throw new DomainException("Este alumno ya está vinculado a esta cuenta.");

        await _padreFamiliaRepository.AddVinculoAsync(new PadreAlumno(padreFamiliaId, alumnoId), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DesvincularAsync(Guid padreFamiliaId, Guid alumnoId, CancellationToken cancellationToken = default)
    {
        var vinculos = await _padreFamiliaRepository.GetVinculosAsync(padreFamiliaId, cancellationToken);
        var vinculo = vinculos.FirstOrDefault(v => v.AlumnoId == alumnoId)
            ?? throw new KeyNotFoundException("El alumno no está vinculado a esta cuenta.");

        await _padreFamiliaRepository.RemoveVinculoAsync(vinculo, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>Listado de cuentas de padres con sus hijos vinculados (issue #8).</summary>
public class ObtenerPadresFamiliaUseCase
{
    private readonly IPadreFamiliaRepository _padreFamiliaRepository;
    private readonly IAlumnoRepository _alumnoRepository;

    public ObtenerPadresFamiliaUseCase(
        IPadreFamiliaRepository padreFamiliaRepository,
        IAlumnoRepository alumnoRepository)
    {
        _padreFamiliaRepository = padreFamiliaRepository;
        _alumnoRepository = alumnoRepository;
    }

    public async Task<IReadOnlyList<PadreFamiliaDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var padres = await _padreFamiliaRepository.GetAllAsync(cancellationToken);
        var alumnos = await _alumnoRepository.GetAllAsync(cancellationToken);
        var porId = alumnos.ToDictionary(a => a.Id);

        var resultado = new List<PadreFamiliaDto>(padres.Count);

        foreach (var padre in padres)
        {
            var alumnoIds = await _padreFamiliaRepository.GetAlumnoIdsAsync(padre.Id, cancellationToken);

            resultado.Add(new PadreFamiliaDto
            {
                Id = padre.Id,
                Email = padre.Email,
                Nombre = padre.Nombre,
                Activo = padre.Activo,
                FechaCreacion = padre.FechaCreacion,
                Hijos = alumnoIds
                    .Where(porId.ContainsKey)
                    .Select(id => new HijoVinculadoDto
                    {
                        Id = id,
                        NumeroMatricula = porId[id].NumeroMatricula,
                        NombreCompleto = $"{porId[id].Nombre} {porId[id].Apellido}"
                    })
                    .ToList()
            });
        }

        return resultado;
    }
}

/// <summary>Activa o desactiva una cuenta de padre (issue #8).</summary>
public class CambiarEstadoPadreFamiliaUseCase
{
    private readonly IPadreFamiliaRepository _padreFamiliaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CambiarEstadoPadreFamiliaUseCase(
        IPadreFamiliaRepository padreFamiliaRepository,
        IUnitOfWork unitOfWork)
    {
        _padreFamiliaRepository = padreFamiliaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync(Guid padreFamiliaId, bool activo, CancellationToken cancellationToken = default)
    {
        var padre = await _padreFamiliaRepository.GetByIdAsync(padreFamiliaId, cancellationToken)
            ?? throw new KeyNotFoundException($"Padre de familia con id '{padreFamiliaId}' no encontrado.");

        if (activo) padre.Reactivar();
        else padre.Desactivar();

        await _padreFamiliaRepository.UpdateAsync(padre, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public class PadreFamiliaDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public IReadOnlyList<HijoVinculadoDto> Hijos { get; set; } = new List<HijoVinculadoDto>();
}

public class HijoVinculadoDto
{
    public Guid Id { get; set; }
    public string NumeroMatricula { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
}
