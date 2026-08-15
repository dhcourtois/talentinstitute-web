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

public class CrearPaceUseCaseTests
{
    private readonly Mock<IPaceRepository> _paceRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CrearPaceUseCase _useCase;

    public CrearPaceUseCaseTests()
    {
        _paceRepositoryMock = new Mock<IPaceRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _useCase = new CrearPaceUseCase(_paceRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_PaceValido_AgregaYGuarda()
    {
        // Arrange
        _paceRepositoryMock
            .Setup(r => r.GetByMateriaYNumeroAsync("MAT", 1045, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pace?)null);

        // Act
        var id = await _useCase.ExecuteAsync(" mat ", 1045, 100, 80);

        // Assert
        id.Should().NotBeEmpty();
        _paceRepositoryMock.Verify(
            r => r.AddAsync(
                It.Is<Pace>(p =>
                    p.Materia == "MAT" &&
                    p.Numero == 1045 &&
                    p.PuntajeMaximo == 100 &&
                    p.PuntajeMinimoAprobacion == 80),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_PaceDuplicado_LanzaDomainException()
    {
        // Arrange
        _paceRepositoryMock
            .Setup(r => r.GetByMateriaYNumeroAsync("MAT", 1045, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Pace("MAT", 1045));

        // Act
        Func<Task> action = async () => await _useCase.ExecuteAsync("MAT", 1045, 100, 80);

        // Assert
        await action.Should().ThrowAsync<DomainException>();
        _paceRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Pace>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("", 1045, 100, 80)]
    [InlineData("MAT", 0, 100, 80)]
    [InlineData("MAT", 1045, 0, 80)]
    [InlineData("MAT", 1045, 100, 101)]
    public async Task ExecuteAsync_DatosInvalidos_LanzaDomainException(
        string materia,
        int numeroPace,
        int puntajeMaximo,
        int puntajeMinimoAprobacion)
    {
        // Act
        Func<Task> action = async () =>
            await _useCase.ExecuteAsync(materia, numeroPace, puntajeMaximo, puntajeMinimoAprobacion);

        // Assert
        await action.Should().ThrowAsync<DomainException>();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
