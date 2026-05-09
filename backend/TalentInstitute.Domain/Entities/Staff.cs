using TalentInstitute.Domain.Exceptions;

namespace TalentInstitute.Domain.Entities;

public class Staff
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public Rol Rol { get; private set; }
    public bool Activo { get; private set; }

    public Staff(string email, string passwordHash, Rol rol)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("El email del staff no puede estar vacío.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("El hash de contraseña no puede estar vacío.");

        Id = Guid.NewGuid();
        Email = email.ToLowerInvariant();
        PasswordHash = passwordHash;
        Rol = rol;
        Activo = true;
    }

    public void Desactivar() => Activo = false;

    public void ActualizarRol(Rol nuevoRol) => Rol = nuevoRol;
}
