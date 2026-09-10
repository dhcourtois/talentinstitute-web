using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Application.UseCases.Portal;
using TalentInstitute.Domain.Entities;
using Xunit;

namespace TalentInstitute.Application.Tests.UseCases.Portal;

/// <summary>
/// El control que impide que un padre lea el expediente de un hijo ajeno.
/// Es la superficie más sensible del portal: el `alumnoId` viaja en la URL y
/// quien está autenticado puede cambiarlo a mano.
/// </summary>
public class VerificarAccesoDelPadreUseCaseTests
{
    private readonly Mock<IPadreFamiliaRepository> _repoMock = new();

    private static readonly Guid PadreId = Guid.NewGuid();
    private static readonly Guid HijoPropio = Guid.NewGuid();
    private static readonly Guid HijoAjeno = Guid.NewGuid();

    private VerificarAccesoDelPadreUseCase CreateSut() => new(_repoMock.Object);

    private static PadreFamilia PadreActivo() => new("papa@ejemplo.com", "hash", "Juan Pérez");

    private void ConPadre(PadreFamilia? padre) =>
        _repoMock.Setup(r => r.GetByIdAsync(PadreId, It.IsAny<CancellationToken>())).ReturnsAsync(padre);

    private void ConVinculo(Guid alumnoId, bool vinculado) =>
        _repoMock.Setup(r => r.TieneAlumnoAsync(PadreId, alumnoId, It.IsAny<CancellationToken>())).ReturnsAsync(vinculado);

    [Fact]
    public async Task Ensure_ConHijoPropio_Permite()
    {
        // Arrange
        ConPadre(PadreActivo());
        ConVinculo(HijoPropio, true);

        // Act
        var acto = async () => await CreateSut().EnsureAsync(PadreId, HijoPropio);

        // Assert
        await acto.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Ensure_ConHijoAjeno_Deniega()
    {
        // Arrange: el caso que de verdad importa.
        ConPadre(PadreActivo());
        ConVinculo(HijoAjeno, false);

        // Act
        var acto = async () => await CreateSut().EnsureAsync(PadreId, HijoAjeno);

        // Assert
        await acto.Should().ThrowAsync<AccesoDenegadoException>();
    }

    [Fact]
    public async Task Ensure_ConCuentaDesactivada_DeniegaAunqueElVinculoExista()
    {
        // Arrange: desactivar una cuenta no borra sus vínculos, así que revisar
        // solo la relación dejaría entrar a un padre dado de baja.
        var padre = PadreActivo();
        padre.Desactivar();
        ConPadre(padre);
        ConVinculo(HijoPropio, true);

        // Act
        var acto = async () => await CreateSut().EnsureAsync(PadreId, HijoPropio);

        // Assert
        await acto.Should().ThrowAsync<AccesoDenegadoException>();
    }

    [Fact]
    public async Task Ensure_ConCuentaDesactivada_NiSiquieraConsultaElVinculo()
    {
        // Arrange
        var padre = PadreActivo();
        padre.Desactivar();
        ConPadre(padre);

        // Act
        try { await CreateSut().EnsureAsync(PadreId, HijoPropio); } catch (AccesoDenegadoException) { }

        // Assert: corta antes de tocar la relación.
        _repoMock.Verify(r => r.TieneAlumnoAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Ensure_ConPadreInexistente_Deniega()
    {
        // Arrange: un token válido cuya cuenta ya fue borrada.
        ConPadre(null);
        ConVinculo(HijoPropio, true);

        // Act
        var acto = async () => await CreateSut().EnsureAsync(PadreId, HijoPropio);

        // Assert
        await acto.Should().ThrowAsync<AccesoDenegadoException>();
    }

    [Fact]
    public async Task Ensure_ElMensajeNoDistingueEntreHijoAjenoYAlumnoInexistente()
    {
        // Arrange: mensajes distintos permitirían sondear la matrícula del colegio.
        ConPadre(PadreActivo());
        ConVinculo(HijoAjeno, false);
        var inexistente = Guid.NewGuid();
        ConVinculo(inexistente, false);

        var sut = CreateSut();

        // Act
        var ajeno = await sut.Invoking(s => s.EnsureAsync(PadreId, HijoAjeno)).Should().ThrowAsync<AccesoDenegadoException>();
        var noExiste = await sut.Invoking(s => s.EnsureAsync(PadreId, inexistente)).Should().ThrowAsync<AccesoDenegadoException>();

        // Assert
        ajeno.Which.Message.Should().Be(noExiste.Which.Message);
    }

    [Fact]
    public async Task Ensure_ConsultaElVinculoDelPadreAutenticado_NoElDeOtro()
    {
        // Arrange: la comprobación debe usar la identidad del token, no un id
        // que pudiera venir del cliente.
        ConPadre(PadreActivo());
        ConVinculo(HijoPropio, true);

        // Act
        await CreateSut().EnsureAsync(PadreId, HijoPropio);

        // Assert
        _repoMock.Verify(r => r.TieneAlumnoAsync(PadreId, HijoPropio, It.IsAny<CancellationToken>()), Times.Once);
    }
}
