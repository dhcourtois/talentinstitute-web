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

public class CheckStudentPaceProgressUseCaseTests
{
    private readonly Mock<IPaceRepository> _paceRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CheckStudentPaceProgressUseCase _useCase;

    public CheckStudentPaceProgressUseCaseTests()
    {
        _paceRepositoryMock = new Mock<IPaceRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _useCase = new CheckStudentPaceProgressUseCase(_paceRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_AlumnoPaceInexistente_LanzaException()
    {
        // Arrange
        _paceRepositoryMock.Setup(repo => repo.GetAlumnoPaceByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AlumnoPace)null);

        // Act
        Func<Task> action = async () => await _useCase.ExecuteAsync(Guid.NewGuid(), true);

        // Assert
        await action.Should().ThrowAsync<Exception>().WithMessage("AlumnoPace no encontrado.");
    }

    [Fact]
    public async Task ExecuteAsync_EstadoInvalido_NoLlamaASaveChanges_LanzaDomainException()
    {
        // Arrange
        var alumnoPace = new AlumnoPace(Guid.NewGuid(), Guid.NewGuid(), "MAT");
        // Estado inicial es Asignado, lo cual es inválido para CompletarAutoTest
        
        _paceRepositoryMock.Setup(repo => repo.GetAlumnoPaceByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(alumnoPace);

        // Act
        Func<Task> action = async () => await _useCase.ExecuteAsync(alumnoPace.Id, true);

        // Assert
        await action.Should().ThrowAsync<DomainException>();
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_EstadoValido_LlamaASaveChanges()
    {
        // Arrange
        var alumnoPace = new AlumnoPace(Guid.NewGuid(), Guid.NewGuid(), "MAT");
        // Forzamos el estado a ListoParaAutoTest (simulación)
        // Ya que no hay setter público, podemos usar reflection para este test o llamar a un método que lo ponga en ese estado.
        // Como no tenemos el método directo sin Meta, usamos reflection para el test.
        var property = typeof(AlumnoPace).GetProperty("Estado");
        property.DeclaringType.GetProperty("Estado").SetValue(alumnoPace, PaceEstado.ListoParaAutoTest, null);

        _paceRepositoryMock.Setup(repo => repo.GetAlumnoPaceByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(alumnoPace);

        // Act
        await _useCase.ExecuteAsync(alumnoPace.Id, true);

        // Assert
        alumnoPace.Estado.Should().Be(PaceEstado.AutoTestOk);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
