using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Application.Interfaces;

namespace TalentInstitute.Application.UseCases;

public class ObtenerMeritosAlumnoUseCase
{
    private readonly IMeritoRepository _meritoRepository;
    private readonly IStaffRepository _staffRepository;

    public ObtenerMeritosAlumnoUseCase(IMeritoRepository meritoRepository, IStaffRepository staffRepository)
    {
        _meritoRepository = meritoRepository;
        _staffRepository = staffRepository;
    }

    public async Task<IReadOnlyList<MeritoDto>> ExecuteAsync(Guid alumnoId, CancellationToken cancellationToken = default)
    {
        var meritos = await _meritoRepository.GetByAlumnoIdAsync(alumnoId, cancellationToken);
        var resultList = new List<MeritoDto>();

        foreach (var m in meritos)
        {
            // Cargar datos de Staff responsable
            var staff = await _staffRepository.GetByIdAsync(m.StaffId, cancellationToken);
            string staffName = "Personal Escolar";
            if (staff != null)
            {
                // Si la clase Staff tiene propiedades Nombre y Apellido
                staffName = $"{staff.Email}"; // Fallback si no tiene nombre/apellido en base, pero revisemos si Staff tiene Nombre
            }
            
            string staffRevocoName = string.Empty;
            if (m.StaffIdRevoco.HasValue)
            {
                var staffRevoco = await _staffRepository.GetByIdAsync(m.StaffIdRevoco.Value, cancellationToken);
                if (staffRevoco != null)
                {
                    staffRevocoName = $"{staffRevoco.Email}";
                }
            }

            resultList.Add(new MeritoDto
            {
                Id = m.Id,
                AlumnoId = m.AlumnoId,
                StaffId = m.StaffId,
                StaffName = staffName,
                Tipo = m.Tipo.ToString(),
                Puntos = m.Puntos,
                Motivo = m.Motivo,
                FechaAplicado = m.FechaAplicado,
                Revocado = m.Revocado,
                StaffIdRevoco = m.StaffIdRevoco,
                StaffRevocoName = staffRevocoName,
                FechaRevocacion = m.FechaRevocacion
            });
        }

        return resultList.OrderByDescending(m => m.FechaAplicado).ToList();
    }
}

public class MeritoDto
{
    public Guid Id { get; set; }
    public Guid AlumnoId { get; set; }
    public Guid StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public int Puntos { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public DateTime FechaAplicado { get; set; }
    public bool Revocado { get; set; }
    public Guid? StaffIdRevoco { get; set; }
    public string StaffRevocoName { get; set; } = string.Empty;
    public DateTime? FechaRevocacion { get; set; }
}
