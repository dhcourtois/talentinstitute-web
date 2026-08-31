using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Application.Interfaces;

namespace TalentInstitute.Application.UseCases;

public class EditarAlumnoUseCase
{
    private readonly IAlumnoRepository _alumnoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EditarAlumnoUseCase(IAlumnoRepository alumnoRepository, IUnitOfWork unitOfWork)
    {
        _alumnoRepository = alumnoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync(
        Guid id,
        string nombre,
        string apellido,
        string nivel,
        CancellationToken cancellationToken = default)
    {
        var alumno = await _alumnoRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Alumno con id '{id}' no encontrado.");

        alumno.ActualizarDatos(nombre, apellido, nivel);

        await _alumnoRepository.UpdateAsync(alumno, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
