using FluentAssertions;
using TalentInstitute.Domain.Entities;
using TalentInstitute.Domain.Exceptions;
using Xunit;

namespace TalentInstitute.Domain.Tests.Entities;

public class AlumnoTests
{
    private readonly ConfiguracionPrivilegios _configPredeterminada = new();

    private static Alumno CrearAlumno() => new("MAT-001", "Test", "Apellido", "1 Primaria");

    [Fact]
    public void RecalcularPrivilegios_DadoBalanceCeroConConfigPredeterminada_PrivilegiosBasicosActivos()
    {
        // Arrange
        var alumno = CrearAlumno();

        // Act
        alumno.RecalcularPrivilegios(0, _configPredeterminada);

        // Assert
        alumno.PrivilegeStatus.Oficina.Should().BeTrue();
        alumno.PrivilegeStatus.Comedor.Should().BeTrue();
        alumno.PrivilegeStatus.Patio.Should().BeTrue();
        alumno.PrivilegeStatus.Biblioteca.Should().BeFalse();
        alumno.PrivilegeStatus.Actividades.Should().BeFalse();
    }

    [Fact]
    public void RecalcularPrivilegios_DadoBalancePorDebajoDeUmbralRevocacionOficina_SeRevoca()
    {
        // Arrange
        var alumno = CrearAlumno();
        // Balance 0: tiene oficina
        alumno.RecalcularPrivilegios(0, _configPredeterminada);

        // Act
        alumno.RecalcularPrivilegios(-2, _configPredeterminada); // El umbral de revocación es -2

        // Assert
        alumno.PrivilegeStatus.Oficina.Should().BeFalse();
    }

    [Fact]
    public void RecalcularPrivilegios_DadoBalancePorDebajoDeUmbralRevocacionPatio_SeRevoca()
    {
        // Arrange
        var alumno = CrearAlumno();

        // Act
        alumno.RecalcularPrivilegios(-5, _configPredeterminada); // Umbral patio es -5

        // Assert
        alumno.PrivilegeStatus.Patio.Should().BeFalse();
    }

    [Fact]
    public void RecalcularPrivilegios_DadoBalanceIgualOSuperiorAUmbralBiblioteca_SeActiva()
    {
        // Arrange
        var alumno = CrearAlumno();

        // Act
        alumno.RecalcularPrivilegios(3, _configPredeterminada); // Umbral biblioteca es 3

        // Assert
        alumno.PrivilegeStatus.Biblioteca.Should().BeTrue();
    }

    [Fact]
    public void RecalcularPrivilegios_DadoBalanceIgualOSuperiorAUmbralActividades_SeActiva()
    {
        // Arrange
        var alumno = CrearAlumno();

        // Act
        alumno.RecalcularPrivilegios(5, _configPredeterminada); // Umbral actividades es 5

        // Assert
        alumno.PrivilegeStatus.Actividades.Should().BeTrue();
    }

    [Fact]
    public void RecalcularPrivilegios_DadoConfigPersonalizada_LosUmbralesPersonalizadosSeRespetan()
    {
        // Arrange
        var alumno = CrearAlumno();
        var configPersonalizada = new ConfiguracionPrivilegios(
            umbralBiblioteca: 10,
            umbralBibliotecaRevocado: 5
        );

        // Act
        alumno.RecalcularPrivilegios(9, configPersonalizada);

        // Assert
        alumno.PrivilegeStatus.Biblioteca.Should().BeFalse(); // Todavía no llega a 10

        // Act 2
        alumno.RecalcularPrivilegios(10, configPersonalizada);

        // Assert 2
        alumno.PrivilegeStatus.Biblioteca.Should().BeTrue(); // Llegó a 10
    }

    [Fact]
    public void ActualizarDatos_DadoDatosValidos_ActualizaNombreApellidoYNivel()
    {
        // Arrange
        var alumno = CrearAlumno();

        // Act
        alumno.ActualizarDatos("Nuevo", "Apellido Nuevo", "2 Primaria");

        // Assert
        alumno.Nombre.Should().Be("Nuevo");
        alumno.Apellido.Should().Be("Apellido Nuevo");
        alumno.Nivel.Should().Be("2 Primaria");
    }

    [Fact]
    public void ActualizarDatos_NoModificaLaMatricula()
    {
        // Arrange
        var alumno = CrearAlumno();

        // Act
        alumno.ActualizarDatos("Nuevo", "Apellido Nuevo", "2 Primaria");

        // Assert
        alumno.NumeroMatricula.Should().Be("MAT-001");
    }

    [Theory]
    [InlineData("", "Apellido", "Nivel")]
    [InlineData("   ", "Apellido", "Nivel")]
    [InlineData("Nombre", "", "Nivel")]
    [InlineData("Nombre", "Apellido", "")]
    public void ActualizarDatos_DadoCampoVacio_LanzaDomainException(string nombre, string apellido, string nivel)
    {
        // Arrange
        var alumno = CrearAlumno();

        // Act
        var acto = () => alumno.ActualizarDatos(nombre, apellido, nivel);

        // Assert
        acto.Should().Throw<DomainException>();
    }

    // ── Fecha de ingreso editable (issue #22) ────────────────────────────────

    [Fact]
    public void Constructor_SinFechaIngreso_TomaLaFechaDeAlta()
    {
        // Act
        var alumno = CrearAlumno();

        // Assert
        alumno.FechaIngreso.Should().Be(DateTime.UtcNow.Date);
    }

    [Fact]
    public void Constructor_ConFechaIngreso_RespetaLaFechaIndicada()
    {
        // Arrange
        var ingreso = new DateTime(2019, 8, 26);

        // Act
        var alumno = new Alumno("MAT-002", "Test", "Apellido", "1 Primaria", ingreso);

        // Assert
        alumno.FechaIngreso.Should().Be(ingreso);
    }

    [Fact]
    public void ActualizarDatos_ConFechaIngreso_LaModifica()
    {
        // Arrange
        var alumno = CrearAlumno();
        var nuevaFecha = new DateTime(2021, 1, 11);

        // Act
        alumno.ActualizarDatos("Test", "Apellido", "1 Primaria", nuevaFecha);

        // Assert
        alumno.FechaIngreso.Should().Be(nuevaFecha);
    }

    [Fact]
    public void ActualizarDatos_SinFechaIngreso_DejaLaFechaComoEstaba()
    {
        // Arrange: quien edita solo el nivel no debe mover una fecha que no tocó.
        var ingreso = new DateTime(2019, 8, 26);
        var alumno = new Alumno("MAT-002", "Test", "Apellido", "1 Primaria", ingreso);

        // Act
        alumno.ActualizarDatos("Test", "Apellido", "2 Primaria");

        // Assert
        alumno.FechaIngreso.Should().Be(ingreso);
    }

    [Fact]
    public void FechaIngreso_SeGuardaSinHora()
    {
        // Arrange: es un dato de calendario, no un instante.
        var conHora = new DateTime(2019, 8, 26, 15, 42, 7);

        // Act
        var alumno = new Alumno("MAT-002", "Test", "Apellido", "1 Primaria", conHora);

        // Assert
        alumno.FechaIngreso.Should().Be(new DateTime(2019, 8, 26));
    }

    [Fact]
    public void ActualizarDatos_ConFechaIngresoInvalida_LanzaDomainException()
    {
        // Arrange
        var alumno = CrearAlumno();

        // Act
        var acto = () => alumno.ActualizarDatos("Test", "Apellido", "1 Primaria", new DateTime(1899, 12, 31));

        // Assert
        acto.Should().Throw<DomainException>();
    }
}
