using FluentAssertions;
using TalentInstitute.Domain.Entities;
using TalentInstitute.Domain.Exceptions;
using Xunit;

namespace TalentInstitute.Domain.Tests.Entities;

public class MetaTests
{
    private static Meta CrearMetaEnProgreso()
    {
        var meta = new Meta(Guid.NewGuid(), Turno.Mañana, 1, 10, DateOnly.FromDateTime(DateTime.Today));
        meta.IniciarProgreso();
        return meta;
    }

    [Fact]
    public void Constructor_ConDatosValidos_EstadoIniciaPendiente()
    {
        var meta = new Meta(Guid.NewGuid(), Turno.Mañana, 1, 10, DateOnly.FromDateTime(DateTime.Today));
        meta.Estado.Should().Be(EstadoMeta.Pendiente);
    }

    [Fact]
    public void Constructor_PaginaInicialMenorOIgualACero_LanzaDomainException()
    {
        Action action = () => new Meta(Guid.NewGuid(), Turno.Tarde, 0, 5, DateOnly.FromDateTime(DateTime.Today));
        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void IniciarProgreso_DesdePendiente_CambiaAEnProgreso()
    {
        var meta = new Meta(Guid.NewGuid(), Turno.Mañana, 1, 5, DateOnly.FromDateTime(DateTime.Today));
        meta.IniciarProgreso();
        meta.Estado.Should().Be(EstadoMeta.EnProgreso);
    }

    [Fact]
    public void Completar_DesdeEnProgreso_CambiaACompletada()
    {
        var meta = CrearMetaEnProgreso();
        meta.Completar();
        meta.Estado.Should().Be(EstadoMeta.Completada);
    }

    [Fact]
    public void Rechazar_DesdeEnProgreso_CambiaARechazada()
    {
        var meta = CrearMetaEnProgreso();
        meta.Rechazar();
        meta.Estado.Should().Be(EstadoMeta.Rechazada);
    }

    [Fact]
    public void RetomarTrasRechazo_DesdeRechazada_CambiaAEnProgreso()
    {
        var meta = CrearMetaEnProgreso();
        meta.Rechazar();
        meta.RetomarTrasRechazo();
        meta.Estado.Should().Be(EstadoMeta.EnProgreso);
    }

    [Fact]
    public void RegistrarScore_DesdeCompletada_CambiaAScored()
    {
        var meta = CrearMetaEnProgreso();
        meta.Completar();
        meta.RegistrarScore(85, 100);
        meta.Estado.Should().Be(EstadoMeta.Scored);
        meta.PuntajeObtenido.Should().Be(85);
    }

    [Fact]
    public void RegistrarScore_PuntajeExcedePuntajeMaximo_LanzaDomainException()
    {
        var meta = CrearMetaEnProgreso();
        meta.Completar();
        Action action = () => meta.RegistrarScore(101, 100);
        action.Should().Throw<DomainException>()
            .WithMessage("El puntaje obtenido no puede exceder el puntaje máximo del PACE.");
    }

    [Fact]
    public void Aprobar_DesdeScored_CambiaAAprobada()
    {
        var meta = CrearMetaEnProgreso();
        meta.Completar();
        meta.RegistrarScore(90, 100);
        meta.Aprobar();
        meta.Estado.Should().Be(EstadoMeta.Aprobada);
    }

    [Fact]
    public void RechazarTrasScore_DesdeScored_CambiaARechazada()
    {
        var meta = CrearMetaEnProgreso();
        meta.Completar();
        meta.RegistrarScore(40, 100);
        meta.RechazarTrasScore();
        meta.Estado.Should().Be(EstadoMeta.Rechazada);
    }

    [Fact]
    public void Completar_DesdeEstadoInvalido_LanzaDomainException()
    {
        var meta = new Meta(Guid.NewGuid(), Turno.Tarde, 1, 5, DateOnly.FromDateTime(DateTime.Today));
        // Estado es Pendiente, no EnProgreso
        Action action = () => meta.Completar();
        action.Should().Throw<DomainException>();
    }

    // ── Rango de páginas (issue #6) ──────────────────────────────────────────

    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.Today);

    [Fact]
    public void Constructor_ConRango_GuardaAmbosExtremos()
    {
        var meta = new Meta(Guid.NewGuid(), Turno.Mañana, 1, 5, Hoy);

        meta.PaginaInicial.Should().Be(1);
        meta.PaginaFinal.Should().Be(5);
    }

    [Theory]
    [InlineData(1, 5, 5)]
    [InlineData(1, 20, 20)]
    [InlineData(10, 14, 5)]
    [InlineData(7, 7, 1)]
    public void PaginasObjetivo_SeDerivaDelRangoYEsInclusiva(int inicial, int final, int esperado)
    {
        var meta = new Meta(Guid.NewGuid(), Turno.Mañana, inicial, final, Hoy);

        meta.PaginasObjetivo.Should().Be(esperado);
    }

    [Fact]
    public void Constructor_ConUnaSolaPagina_EsValido()
    {
        // El issue lo pide explícitamente: página 5 a página 5 sigue siendo válido.
        var acto = () => new Meta(Guid.NewGuid(), Turno.Tarde, 5, 5, Hoy);

        acto.Should().NotThrow();
    }

    [Fact]
    public void Constructor_ConPaginaFinalMenorQueLaInicial_LanzaDomainException()
    {
        var acto = () => new Meta(Guid.NewGuid(), Turno.Tarde, 5, 4, Hoy);

        acto.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Constructor_ConPaginaInicialInvalida_LanzaDomainException(int inicial)
    {
        var acto = () => new Meta(Guid.NewGuid(), Turno.Tarde, inicial, 10, Hoy);

        acto.Should().Throw<DomainException>();
    }

    [Fact]
    public void ValidarRangoContraPace_CuandoElRangoCabe_NoLanza()
    {
        var acto = () => Meta.ValidarRangoContraPace(1, 20, totalPaginasDelPace: 40);

        acto.Should().NotThrow();
    }

    [Fact]
    public void ValidarRangoContraPace_CuandoElRangoTerminaJustoEnElTotal_NoLanza()
    {
        var acto = () => Meta.ValidarRangoContraPace(30, 40, totalPaginasDelPace: 40);

        acto.Should().NotThrow();
    }

    [Fact]
    public void ValidarRangoContraPace_CuandoElRangoSePasa_LanzaDomainException()
    {
        var acto = () => Meta.ValidarRangoContraPace(35, 45, totalPaginasDelPace: 40);

        acto.Should().Throw<DomainException>();
    }

    [Fact]
    public void ValidarRangoContraPace_SinTotalCapturado_NoLanza()
    {
        // Los PACEs anteriores al issue #6 no tienen el total; sin él no hay
        // contra qué validar y la meta no debe bloquearse.
        var acto = () => Meta.ValidarRangoContraPace(1, 999, totalPaginasDelPace: null);

        acto.Should().NotThrow();
    }

    // ── Transiciones desde Pendiente (QA_002, PR #15) ────────────────────────
    // Venían de `main`, que nunca se había fusionado de vuelta a develop. Se
    // conservan tal cual, portadas al constructor de rango: "5 páginas" pasa a
    // ser el rango 1..5, que es la misma cantidad.

    [Fact]
    public void Completar_DesdePendienteTrasIniciarProgreso_CambiaACompletada()
    {
        var meta = new Meta(Guid.NewGuid(), Turno.Mañana, 1, 5, DateOnly.FromDateTime(DateTime.Today));
        meta.Estado.Should().Be(EstadoMeta.Pendiente);
        meta.IniciarProgreso();
        meta.Completar();
        meta.Estado.Should().Be(EstadoMeta.Completada);
    }

    [Fact]
    public void Rechazar_DesdePendienteTrasIniciarProgreso_CambiaARechazada()
    {
        var meta = new Meta(Guid.NewGuid(), Turno.Tarde, 1, 5, DateOnly.FromDateTime(DateTime.Today));
        meta.Estado.Should().Be(EstadoMeta.Pendiente);
        meta.IniciarProgreso();
        meta.Rechazar();
        meta.Estado.Should().Be(EstadoMeta.Rechazada);
    }
}
