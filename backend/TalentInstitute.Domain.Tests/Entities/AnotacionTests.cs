using FluentAssertions;
using TalentInstitute.Domain.Entities;
using TalentInstitute.Domain.Exceptions;
using Xunit;

namespace TalentInstitute.Domain.Tests.Entities;

public class AnotacionTests
{
    private static readonly Guid AlumnoId = Guid.NewGuid();
    private static readonly Guid StaffId = Guid.NewGuid();

    private static Anotacion Crear(string texto = "Avanzó bien en matemáticas.", DateOnly? fecha = null)
        => new(AlumnoId, StaffId, texto, fecha);

    [Fact]
    public void Constructor_DadoTextoValido_CreaLaAnotacion()
    {
        // Act
        var anotacion = Crear();

        // Assert
        anotacion.Id.Should().NotBeEmpty();
        anotacion.AlumnoId.Should().Be(AlumnoId);
        anotacion.StaffId.Should().Be(StaffId);
        anotacion.Texto.Should().Be("Avanzó bien en matemáticas.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void Constructor_DadoTextoVacio_LanzaDomainException(string texto)
    {
        // Act
        var acto = () => Crear(texto);

        // Assert
        acto.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_RecortaLosEspaciosDelTexto()
    {
        // Act
        var anotacion = Crear("   Con espacios alrededor.   ");

        // Assert
        anotacion.Texto.Should().Be("Con espacios alrededor.");
    }

    [Fact]
    public void Constructor_DadoTextoDemasiadoLargo_LanzaDomainException()
    {
        // Arrange: la columna tiene tope; sin esta validación fallaría en la base.
        var excedido = new string('a', Anotacion.LargoMaximo + 1);

        // Act
        var acto = () => Crear(excedido);

        // Assert
        acto.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_DadoElLargoMaximoExacto_NoLanza()
    {
        // Act
        var acto = () => Crear(new string('a', Anotacion.LargoMaximo));

        // Assert
        acto.Should().NotThrow();
    }

    [Fact]
    public void Constructor_SinAlumno_LanzaDomainException()
    {
        // Act
        var acto = () => new Anotacion(Guid.Empty, StaffId, "Texto");

        // Assert
        acto.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_SinAutor_LanzaDomainException()
    {
        // Act
        var acto = () => new Anotacion(AlumnoId, Guid.Empty, "Texto");

        // Assert
        acto.Should().Throw<DomainException>();
    }

    // ── Anclaje semanal ──────────────────────────────────────────────────────

    [Theory]
    // Toda la semana del 7 al 13 de septiembre de 2026 ancla al lunes 7.
    [InlineData(2026, 9, 7, 2026, 9, 7)]   // lunes
    [InlineData(2026, 9, 9, 2026, 9, 7)]   // miércoles
    [InlineData(2026, 9, 12, 2026, 9, 7)]  // sábado
    [InlineData(2026, 9, 13, 2026, 9, 7)]  // domingo, el borde que se presta a error
    [InlineData(2026, 9, 14, 2026, 9, 14)] // lunes siguiente, ya es otra semana
    public void SemanaInicio_AnclaAlLunesDeLaSemana(int a, int m, int d, int ea, int em, int ed)
    {
        // Act
        var anotacion = Crear(fecha: new DateOnly(a, m, d));

        // Assert
        anotacion.SemanaInicio.Should().Be(new DateOnly(ea, em, ed));
    }

    [Fact]
    public void SemanaInicio_CruzaElCambioDeMesSinPerderLaSemana()
    {
        // Arrange: el 1 de octubre de 2026 es jueves; su lunes cae en septiembre.
        var jueves = new DateOnly(2026, 10, 1);

        // Act
        var anotacion = Crear(fecha: jueves);

        // Assert
        anotacion.SemanaInicio.Should().Be(new DateOnly(2026, 9, 28));
    }

    [Fact]
    public void InicioDeSemana_EsIdempotente()
    {
        // Arrange: aplicarlo sobre un lunes debe devolver el mismo lunes.
        var lunes = Anotacion.InicioDeSemana(new DateOnly(2026, 9, 9));

        // Act
        var otraVez = Anotacion.InicioDeSemana(lunes);

        // Assert
        otraVez.Should().Be(lunes);
    }

    [Fact]
    public void Constructor_SinFecha_UsaLaSemanaEnCurso()
    {
        // Act
        var anotacion = Crear();

        // Assert
        var esperado = Anotacion.InicioDeSemana(DateOnly.FromDateTime(DateTime.UtcNow));
        anotacion.SemanaInicio.Should().Be(esperado);
    }
}
