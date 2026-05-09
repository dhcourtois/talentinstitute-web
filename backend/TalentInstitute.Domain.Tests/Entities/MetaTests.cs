using FluentAssertions;
using TalentInstitute.Domain.Entities;
using TalentInstitute.Domain.Exceptions;
using Xunit;

namespace TalentInstitute.Domain.Tests.Entities;

public class MetaTests
{
    private static Meta CrearMetaEnProgreso()
    {
        var meta = new Meta(Guid.NewGuid(), Turno.Mañana, 10, DateOnly.FromDateTime(DateTime.Today));
        meta.IniciarProgreso();
        return meta;
    }

    [Fact]
    public void Constructor_ConDatosValidos_EstadoIniciaPendiente()
    {
        var meta = new Meta(Guid.NewGuid(), Turno.Mañana, 10, DateOnly.FromDateTime(DateTime.Today));
        meta.Estado.Should().Be(EstadoMeta.Pendiente);
    }

    [Fact]
    public void Constructor_PaginasObjetivoMenorOIgualACero_LanzaDomainException()
    {
        Action action = () => new Meta(Guid.NewGuid(), Turno.Tarde, 0, DateOnly.FromDateTime(DateTime.Today));
        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void IniciarProgreso_DesdePendiente_CambiaAEnProgreso()
    {
        var meta = new Meta(Guid.NewGuid(), Turno.Mañana, 5, DateOnly.FromDateTime(DateTime.Today));
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
        var meta = new Meta(Guid.NewGuid(), Turno.Tarde, 5, DateOnly.FromDateTime(DateTime.Today));
        // Estado es Pendiente, no EnProgreso
        Action action = () => meta.Completar();
        action.Should().Throw<DomainException>();
    }
}
