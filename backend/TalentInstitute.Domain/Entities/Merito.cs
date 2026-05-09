using TalentInstitute.Domain.Exceptions;

namespace TalentInstitute.Domain.Entities;

public class Merito
{
    public Guid Id { get; private set; }
    public Guid AlumnoId { get; private set; }
    public Guid StaffId { get; private set; }
    public TipoMerito Tipo { get; private set; }
    public int Puntos { get; private set; }
    public string Motivo { get; private set; }
    public DateTime FechaAplicado { get; private set; }
    public bool Revocado { get; private set; }
    public Guid? StaffIdRevoco { get; private set; }
    public DateTime? FechaRevocacion { get; private set; }
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public Merito(Guid alumnoId, Guid staffId, TipoMerito tipo, int puntos, string motivo)
    {
        if (puntos <= 0)
            throw new DomainException("Los puntos del mérito deben ser mayor a 0.");

        if (string.IsNullOrWhiteSpace(motivo))
            throw new DomainException("El motivo del mérito no puede estar vacío.");

        Id = Guid.NewGuid();
        AlumnoId = alumnoId;
        StaffId = staffId;
        Tipo = tipo;
        Puntos = puntos;
        Motivo = motivo;
        FechaAplicado = DateTime.UtcNow;
        Revocado = false;
    }

    public void Revocar(Guid staffIdRevoco)
    {
        if (Revocado)
            throw new DomainException("Este registro ya fue revocado.");

        Revocado = true;
        StaffIdRevoco = staffIdRevoco;
        FechaRevocacion = DateTime.UtcNow;
    }
}
