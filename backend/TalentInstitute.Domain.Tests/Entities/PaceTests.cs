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
        var pace = new Pace("MAT", "1097");
        pace.Materia.Should().Be("MAT");
        pace.Numero.Should().Be("1097");
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

    // ── Número alfanumérico ──────────────────────────────────────────────────

    [Fact]
    public void Constructor_ConNumeroAlfanumerico_LoAcepta()
    {
        // El caso que motivó el cambio: el colegio captura PACEs tipo RR01.
        var pace = new Pace("MAT", "RR01");

        pace.Numero.Should().Be("RR01");
    }

    [Theory]
    [InlineData("1045")]
    [InlineData("RR01")]
    [InlineData("MAT1045")]
    [InlineData("0")]
    public void Constructor_ConFormatosValidos_NoLanza(string numero)
    {
        // "1045" sigue funcionando: no romper lo que ya existía es el punto.
        var acto = () => new Pace("MAT", numero);

        acto.Should().NotThrow();
    }

    [Theory]
    [InlineData("rr01", "RR01")]
    [InlineData("  RR01  ", "RR01")]
    [InlineData(" mat1045 ", "MAT1045")]
    public void Constructor_NormalizaElNumero(string entrada, string esperado)
    {
        // Sin esta normalización, "rr01" y "RR01" serían dos PACEs distintos y
        // la comprobación de duplicados no los vería como el mismo.
        var pace = new Pace("MAT", entrada);

        pace.Numero.Should().Be(esperado);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("RR-01")]
    [InlineData("RR 01")]
    [InlineData("RR_01")]
    [InlineData("RR.01")]
    [InlineData("RR/01")]
    public void Constructor_ConNumeroInvalido_LanzaDomainException(string numero)
    {
        var acto = () => new Pace("MAT", numero);

        acto.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_ConNumeroDemasiadoLargo_LanzaDomainException()
    {
        // El tope es el largo de la columna; sin la validación fallaría en la base.
        var excedido = new string('A', Pace.LargoMaximoNumero + 1);

        var acto = () => new Pace("MAT", excedido);

        acto.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_ConElLargoMaximoExacto_NoLanza()
    {
        var acto = () => new Pace("MAT", new string('A', Pace.LargoMaximoNumero));

        acto.Should().NotThrow();
    }

    [Fact]
    public void NormalizarNumero_EsIdempotente()
    {
        // Se aplica tanto al crear como al buscar; aplicarla dos veces no debe
        // cambiar el resultado o la búsqueda no encontraría lo recién guardado.
        var unaVez = Pace.NormalizarNumero(" rr01 ");
        var otraVez = Pace.NormalizarNumero(unaVez);

        otraVez.Should().Be(unaVez);
    }
}
