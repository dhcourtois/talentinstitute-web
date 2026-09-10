using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Application.Interfaces;

namespace TalentInstitute.Application.UseCases;

/// <summary>
/// Historial de observaciones de un alumno, más reciente primero. Viene ya
/// ordenado por semana desde el repositorio; la agrupación visual la arma la
/// pantalla a partir de `SemanaInicio`.
/// </summary>
public class ObtenerAnotacionesAlumnoUseCase
{
    private readonly IAnotacionRepository _anotacionRepository;
    private readonly IStaffRepository _staffRepository;

    public ObtenerAnotacionesAlumnoUseCase(
        IAnotacionRepository anotacionRepository,
        IStaffRepository staffRepository)
    {
        _anotacionRepository = anotacionRepository;
        _staffRepository = staffRepository;
    }

    public async Task<IReadOnlyList<AnotacionDto>> ExecuteAsync(Guid alumnoId, CancellationToken cancellationToken = default)
    {
        var anotaciones = await _anotacionRepository.GetByAlumnoIdAsync(alumnoId, cancellationToken);

        // El nombre del autor se resuelve una vez por staff y no una vez por
        // anotación: un alumno con meses de historial repite pocos autores.
        var autores = new Dictionary<Guid, string>();
        var resultado = new List<AnotacionDto>(anotaciones.Count);

        foreach (var anotacion in anotaciones)
        {
            if (!autores.TryGetValue(anotacion.StaffId, out var autor))
            {
                var staff = await _staffRepository.GetByIdAsync(anotacion.StaffId, cancellationToken);
                autor = staff?.Email ?? "Personal Escolar";
                autores[anotacion.StaffId] = autor;
            }

            resultado.Add(new AnotacionDto
            {
                Id = anotacion.Id,
                AlumnoId = anotacion.AlumnoId,
                StaffId = anotacion.StaffId,
                StaffName = autor,
                SemanaInicio = anotacion.SemanaInicio,
                Texto = anotacion.Texto,
                FechaCreacion = anotacion.FechaCreacion
            });
        }

        return resultado;
    }
}

public class AnotacionDto
{
    public Guid Id { get; set; }
    public Guid AlumnoId { get; set; }
    public Guid StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;

    /// <summary>Lunes de la semana a la que pertenece la observación.</summary>
    public DateOnly SemanaInicio { get; set; }

    public string Texto { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
}
