using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Application.UseCases;
using TalentInstitute.Domain.Entities;
using TalentInstitute.Domain.Exceptions;
using Xunit;

namespace TalentInstitute.Application.Tests.UseCases;

public class RegistrarEntrevistaUseCaseTests
{
    private readonly Mock<IEntrevistaRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork>           _uowMock  = new();

    private RegistrarEntrevistaUseCase CreateSut() =>
        new(_repoMock.Object, _uowMock.Object);

    [Fact]
    public async Task ExecuteAsync_ConDatosValidos_DebeGuardarYRetornarId()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var id = await sut.ExecuteAsync(
            nombrePadre:     "María López",
            numeroHijos:     2,
            riesgoViolencia: false,
            riesgoDivorcio:  false,
            conoceADios:     true,
            comentarios:     "Familia estable",
            aceptado:        true);

        // Assert
        id.Should().NotBeEmpty();
        _repoMock.Verify(r => r.AddAsync(It.IsAny<EntrevistaPadre>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ConRiesgoViolenciaYComentariosVacios_DebeLanzarDomainException()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        Func<Task> act = () => sut.ExecuteAsync(
            nombrePadre:     "Carlos Ruiz",
            numeroHijos:     1,
            riesgoViolencia: true,
            riesgoDivorcio:  false,
            conoceADios:     false,
            comentarios:     "   ",   // vacío — debe fallar
            aceptado:        false);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*banderas de riesgo*");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<EntrevistaPadre>(), It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ConNombreVacio_DebeLanzarDomainException()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        Func<Task> act = () => sut.ExecuteAsync(
            nombrePadre:     "",
            numeroHijos:     0,
            riesgoViolencia: false,
            riesgoDivorcio:  false,
            conoceADios:     true,
            comentarios:     "OK",
            aceptado:        true);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*nombre del padre*");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<EntrevistaPadre>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_SiRepositorioFalla_PropagaExcepcion()
    {
        // Arrange
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<EntrevistaPadre>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        var sut = CreateSut();

        // Act
        Func<Task> act = () => sut.ExecuteAsync("Pedro García", 3, false, false, true, "OK", true);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
