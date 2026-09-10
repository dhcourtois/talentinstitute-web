using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Application.UseCases;

public class CrearAlumnoUseCase
{
    private readonly IAlumnoRepository _alumnoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CrearAlumnoUseCase(IAlumnoRepository alumnoRepository, IUnitOfWork unitOfWork)
    {
        _alumnoRepository = alumnoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> ExecuteAsync(
        string numeroMatricula,
        string nombre,
        string apellido,
        string nivel,
        DateTime? fechaIngreso = null,
        CancellationToken cancellationToken = default)
    {
        var alumnos = await _alumnoRepository.GetAllAsync(cancellationToken);
        if (alumnos.Any(a => a.NumeroMatricula.Equals(numeroMatricula, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Ya existe un alumno registrado con la matrícula '{numeroMatricula}'.");
        }

        var alumno = new Alumno(numeroMatricula, nombre, apellido, nivel, fechaIngreso);

        await _alumnoRepository.AddAsync(alumno, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return alumno.Id;
    }
}
