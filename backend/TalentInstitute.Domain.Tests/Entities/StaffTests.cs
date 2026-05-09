using FluentAssertions;
using TalentInstitute.Domain.Entities;
using TalentInstitute.Domain.Exceptions;
using Xunit;

namespace TalentInstitute.Domain.Tests.Entities;

public class StaffTests
{
    [Fact]
    public void Constructor_ConDatosValidos_DebeCrearInstanciaActiva()
    {
        var staff = new Staff("supervisora@talent.com", "hashedPassword", Rol.Supervisora);

        staff.Email.Should().Be("supervisora@talent.com");
        staff.Rol.Should().Be(Rol.Supervisora);
        staff.Activo.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_EmailVacio_DebeLanzarDomainException(string email)
    {
        Action action = () => new Staff(email, "hash", Rol.Monitora);
        action.Should().Throw<DomainException>()
            .WithMessage("El email del staff no puede estar vacío.");
    }

    [Fact]
    public void Constructor_PasswordHashVacio_DebeLanzarDomainException()
    {
        Action action = () => new Staff("a@b.com", "", Rol.Monitora);
        action.Should().Throw<DomainException>()
            .WithMessage("El hash de contraseña no puede estar vacío.");
    }

    [Fact]
    public void Constructor_EmailSeMayusculasConvierteAMinusculas()
    {
        var staff = new Staff("PRINCIPAL@TALENT.COM", "hash", Rol.Principal);
        staff.Email.Should().Be("principal@talent.com");
    }

    [Fact]
    public void Desactivar_CambiaActivoAFalse()
    {
        var staff = new Staff("a@b.com", "hash", Rol.Monitora);
        staff.Desactivar();
        staff.Activo.Should().BeFalse();
    }
}
