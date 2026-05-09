using System;
using FluentAssertions;
using TalentInstitute.Domain.Entities;
using TalentInstitute.Domain.Exceptions;
using Xunit;

namespace TalentInstitute.Domain.Tests.Entities;

public class PaceTests
{
    [Fact]
    public void Constructor_Pace_DebeInicializarValores()
    {
        var pace = new Pace("MAT", 1097);
        pace.Materia.Should().Be("MAT");
        pace.Numero.Should().Be(1097);
        pace.PuntajeMaximo.Should().Be(100);
        pace.PuntajeMinimoAprobacion.Should().Be(80);
    }

    [Fact]
    public void AlumnoPace_EstadoInicial_DebeSerAsignado()
    {
        var alumnoPace = new AlumnoPace(Guid.NewGuid(), Guid.NewGuid(), "MAT");
        alumnoPace.Estado.Should().Be(PaceEstado.Asignado);
    }

    [Fact]
    public void RegistrarPrimeraMeta_DesdeAsignado_CambiaAEnProgreso()
    {
        var alumnoPace = new AlumnoPace(Guid.NewGuid(), Guid.NewGuid(), "MAT");
        alumnoPace.RegistrarPrimeraMeta();
        alumnoPace.Estado.Should().Be(PaceEstado.EnProgreso);
    }

    [Fact]
    public void CompletarAutoTest_EstadoInvalido_LanzaException()
    {
        var alumnoPace = new AlumnoPace(Guid.NewGuid(), Guid.NewGuid(), "MAT");
        // Estado es Asignado, no ListoParaAutoTest
        Action action = () => alumnoPace.CompletarAutoTest(true);
        action.Should().Throw<DomainException>().WithMessage("El PACE no está en estado válido para realizar el auto-test.");
    }

    [Fact]
    public void no_se_puede_asignar_pace_si_ya_existe_uno_activo_para_la_misma_materia()
    {
        var alumnoId = Guid.NewGuid();
        var paceActivo = new AlumnoPace(alumnoId, Guid.NewGuid(), "MAT");
        // Estado Asignado ya es activo

        var pacesExistentes = new[] { paceActivo };

        Action action = () => AlumnoPace.ValidarAsignacionUnica(pacesExistentes, "MAT");
        action.Should().Throw<DomainException>()
            .WithMessage("*PACE activo para la materia 'MAT'*");
    }

    [Fact]
    public void ValidarAsignacionUnica_MateriaDiferente_NoLanzaException()
    {
        var paceActivo = new AlumnoPace(Guid.NewGuid(), Guid.NewGuid(), "MAT");
        var pacesExistentes = new[] { paceActivo };

        // Asignar LEC no debe lanzar aunque MAT esté activo
        Action action = () => AlumnoPace.ValidarAsignacionUnica(pacesExistentes, "LEC");
        action.Should().NotThrow();
    }

    [Fact]
    public void ValidarAsignacionUnica_PaceCompletado_PermiteNuevaAsignacion()
    {
        var alumnoPace = new AlumnoPace(Guid.NewGuid(), Guid.NewGuid(), "MAT");
        // Llevar hasta estado Completado
        alumnoPace.RegistrarPrimeraMeta();
        var estadoProp = typeof(AlumnoPace).GetProperty("Estado");
        estadoProp!.DeclaringType!.GetProperty("Estado")!.SetValue(alumnoPace, PaceEstado.Completado, null);

        var pacesExistentes = new[] { alumnoPace };

        Action action = () => AlumnoPace.ValidarAsignacionUnica(pacesExistentes, "MAT");
        action.Should().NotThrow();
    }
}
