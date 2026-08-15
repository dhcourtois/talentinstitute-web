using FluentAssertions;
using TalentInstitute.Domain.Entities;
using TalentInstitute.Domain.Exceptions;
using Xunit;

namespace TalentInstitute.Domain.Tests.Entities;

public class MeritoTests
{
    [Fact]
    public void no_se_puede_crear_merito_con_puntos_cero_o_negativos()
    {
        Action actionCero = () => new Merito(Guid.NewGuid(), Guid.NewGuid(), TipoMerito.Merito, 0, "Buen comportamiento");
        Action actionNegativo = () => new Merito(Guid.NewGuid(), Guid.NewGuid(), TipoMerito.Merito, -1, "Buen comportamiento");

        actionCero.Should().Throw<DomainException>()
            .WithMessage("Los puntos del mérito deben ser mayor a 0.");
        actionNegativo.Should().Throw<DomainException>()
            .WithMessage("Los puntos del mérito deben ser mayor a 0.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void no_se_puede_crear_merito_sin_motivo(string motivo)
    {
        Action action = () => new Merito(Guid.NewGuid(), Guid.NewGuid(), TipoMerito.Demerito, 1, motivo);
        action.Should().Throw<DomainException>()
            .WithMessage("El motivo del mérito no puede estar vacío.");
    }

    [Fact]
    public void Constructor_ConDatosValidos_CreaInstanciaNoRevocada()
    {
        var merito = new Merito(Guid.NewGuid(), Guid.NewGuid(), TipoMerito.Merito, 5, "Participación destacada");

        merito.Puntos.Should().Be(5);
        merito.Tipo.Should().Be(TipoMerito.Merito);
        merito.Revocado.Should().BeFalse();
        merito.StaffIdRevoco.Should().BeNull();
        merito.FechaRevocacion.Should().BeNull();
    }

    [Fact]
    public void Revocar_MeritoNoRevocado_MarcaComoRevocado()
    {
        var merito = new Merito(Guid.NewGuid(), Guid.NewGuid(), TipoMerito.Demerito, 2, "Llegó tarde");
        var staffRevocador = Guid.NewGuid();

        merito.Revocar(staffRevocador);

        merito.Revocado.Should().BeTrue();
        merito.StaffIdRevoco.Should().Be(staffRevocador);
        merito.FechaRevocacion.Should().NotBeNull();
    }

    [Fact]
    public void Revocar_MeritoYaRevocado_LanzaDomainException()
    {
        var merito = new Merito(Guid.NewGuid(), Guid.NewGuid(), TipoMerito.Merito, 1, "Prueba");
        merito.Revocar(Guid.NewGuid());

        Action action = () => merito.Revocar(Guid.NewGuid());
        action.Should().Throw<DomainException>()
            .WithMessage("Este registro ya fue revocado.");
    }

    [Fact]
    public void Tipo_AceptaSoloMeritoODemerito()
    {
        var merito = new Merito(Guid.NewGuid(), Guid.NewGuid(), TipoMerito.Merito, 1, "test");
        var demerito = new Merito(Guid.NewGuid(), Guid.NewGuid(), TipoMerito.Demerito, 1, "test");

        merito.Tipo.Should().Be(TipoMerito.Merito);
        demerito.Tipo.Should().Be(TipoMerito.Demerito);
    }
}
