using TalentInstitute.Domain.Exceptions;

namespace TalentInstitute.Domain.Entities;

/// <summary>
/// Cuenta de acceso para un padre de familia (issue #8).
///
/// No es un `Staff` con otro rol: el personal del colegio opera el sistema y el
/// padre solo consulta a sus propios hijos. Separarlos en dos entidades evita
/// que un descuido en la matriz de permisos del personal le abra al padre una
/// pantalla de operación.
/// </summary>
public class PadreFamilia
{
    /// <summary>
    /// Valor del claim de rol en el token. No entra al enum `Rol`, que describe
    /// al personal del colegio y alimenta los `[Authorize(Roles = ...)]` de los
    /// controladores internos.
    /// </summary>
    public const string RolToken = "Padre";

    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string Nombre { get; private set; }
    public bool Activo { get; private set; }
    public DateTime FechaCreacion { get; private set; }

    // Requerido por EF Core.
    private PadreFamilia()
    {
        Email = string.Empty;
        PasswordHash = string.Empty;
        Nombre = string.Empty;
    }

    public PadreFamilia(string email, string passwordHash, string nombre)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("El correo del padre de familia no puede estar vacío.");

        if (!email.Contains('@'))
            throw new DomainException("El correo del padre de familia no es válido.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("La credencial del padre de familia no puede estar vacía.");

        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre del padre de familia no puede estar vacío.");

        Id = Guid.NewGuid();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        Nombre = nombre.Trim();
        Activo = true;
        FechaCreacion = DateTime.UtcNow;
    }

    public void Desactivar() => Activo = false;

    public void Reactivar() => Activo = true;

    public void CambiarCredencial(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("La credencial del padre de familia no puede estar vacía.");

        PasswordHash = passwordHash;
    }
}
