using FluentAssertions;
using TalentInstitute.Domain.Entities;
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
}
