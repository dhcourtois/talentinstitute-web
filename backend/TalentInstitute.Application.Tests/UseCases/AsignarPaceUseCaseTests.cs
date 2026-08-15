using System;
using System.Collections.Generic;
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

public class AsignarPaceUseCaseTests
{
    private readonly Mock<IPaceRepository> _paceRepositoryMock;
    private readonly Mock<IAlumnoRepository> _alumnoRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AsignarPaceUseCase _useCase;

    public AsignarPaceUseCaseTests()
    {
        _paceRepositoryMock = new Mock<IPaceRepository>();
        _alumnoRepositoryMock = new Mock<IAlumnoRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _useCase = new AsignarPaceUseCase(
            _paceRepositoryMock.Object,
            _alumnoRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_AlumnoInexistente_LanzaException()
    {
        // Arrange
        _alumnoRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Alumno)null);

        // Act
        Func<Task> action = async () => await _useCase.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>();
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_PaceInexistente_LanzaException()
    {
        // Arrange
        var alumno = new Alumno("MAT-111", "Juan", "Perez", "1 Primaria");
        _alumnoRepositoryMock.Setup(repo => repo.GetByIdAsync(alumno.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(alumno);

        _paceRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pace)null);

        // Act
        Func<Task> action = async () => await _useCase.ExecuteAsync(alumno.Id, Guid.NewGuid());

        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>();
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_AsignacionValida_CreaAlumnoPaceYGuarda()
    {
        // Arrange
        var alumno = new Alumno("MAT-111", "Juan", "Perez", "1 Primaria");
        var pace = new Pace("MAT", 1045);

        _alumnoRepositoryMock.Setup(repo => repo.GetByIdAsync(alumno.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(alumno);

        _paceRepositoryMock.Setup(repo => repo.GetByIdAsync(pace.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pace);

        _paceRepositoryMock.Setup(repo => repo.GetAlumnoPacesByAlumnoIdAsync(alumno.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AlumnoPace>());

        // Act
        var resultId = await _useCase.ExecuteAsync(alumno.Id, pace.Id);

        // Assert
        resultId.Should().NotBeEmpty();
        _paceRepositoryMock.Verify(repo => repo.AddAlumnoPaceAsync(It.IsAny<AlumnoPace>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_MateriaActivaExistente_LanzaDomainException()
    {
        // Arrange
        var alumno = new Alumno("MAT-111", "Juan", "Perez", "1 Primaria");
        var pace = new Pace("MAT", 1045);
        var alumnoPaceExistente = new AlumnoPace(alumno.Id, Guid.NewGuid(), "MAT"); // Estado Asignado por defecto (activo)

        _alumnoRepositoryMock.Setup(repo => repo.GetByIdAsync(alumno.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(alumno);

        _paceRepositoryMock.Setup(repo => repo.GetByIdAsync(pace.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pace);

        _paceRepositoryMock.Setup(repo => repo.GetAlumnoPacesByAlumnoIdAsync(alumno.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AlumnoPace> { alumnoPaceExistente });

        // Act
        Func<Task> action = async () => await _useCase.ExecuteAsync(alumno.Id, pace.Id);

        // Assert
        await action.Should().ThrowAsync<DomainException>();
        _paceRepositoryMock.Verify(repo => repo.AddAlumnoPaceAsync(It.IsAny<AlumnoPace>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
