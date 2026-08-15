using FluentAssertions;
using Moq;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Application.UseCases;
using TalentInstitute.Domain.Entities;
using Xunit;

namespace TalentInstitute.Application.Tests.UseCases;

public class RegistrarMeritoUseCaseTests
{
    private readonly Mock<IMeritoRepository> _meritoRepoMock = new();
    private readonly Mock<IAlumnoRepository> _alumnoRepoMock = new();
    private readonly Mock<IConfiguracionRepository> _configRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly RegistrarMeritoUseCase _useCase;

    private static readonly ConfiguracionPrivilegios _configDefault = new();

    public RegistrarMeritoUseCaseTests()
    {
        _useCase = new RegistrarMeritoUseCase(
            _meritoRepoMock.Object,
            _alumnoRepoMock.Object,
            _configRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_MeritoValido_LlamaASaveAsyncYUpdateAsync()
    {
        // Arrange
        var alumno = new Alumno("MAT-001", "Ana", "García", "2 Primaria");
        _alumnoRepoMock.Setup(r => r.GetByIdAsync(alumno.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(alumno);
        _configRepoMock.Setup(r => r.GetActivaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_configDefault);

        // Act
        var meritoId = await _useCase.ExecuteAsync(alumno.Id, Guid.NewGuid(), TipoMerito.Merito, 3, "Excelente participación");

        // Assert — obligatorio per spec: se llama a AMBOS repositorios
        _meritoRepoMock.Verify(r => r.AddAsync(It.IsAny<Merito>(), It.IsAny<CancellationToken>()), Times.Once);
        _alumnoRepoMock.Verify(r => r.UpdateAsync(It.Is<Alumno>(a => a.Id == alumno.Id), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        meritoId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_AlumnoNoEncontrado_LanzaExcepcionSinGuardarNada()
    {
        // Arrange
        _alumnoRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Alumno?)null);

        // Act
        Func<Task> action = async () => await _useCase.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid(), TipoMerito.Demerito, 1, "Prueba");

        // Assert — spec: propaga el error sin estado parcial guardado
        await action.Should().ThrowAsync<Exception>();
        _meritoRepoMock.Verify(r => r.AddAsync(It.IsAny<Merito>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_RepositorioLanzaExcepcion_NoPersisteParcialmente()
    {
        // Arrange
        var alumno = new Alumno("MAT-002", "Luis", "Torres", "3 Primaria");
        _alumnoRepoMock.Setup(r => r.GetByIdAsync(alumno.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(alumno);
        _configRepoMock.Setup(r => r.GetActivaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_configDefault);
        _meritoRepoMock.Setup(r => r.AddAsync(It.IsAny<Merito>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));

        // Act
        Func<Task> action = async () => await _useCase.ExecuteAsync(alumno.Id, Guid.NewGuid(), TipoMerito.Demerito, 2, "Motivo");

        // Assert — el error se propaga correctamente
        await action.Should().ThrowAsync<Exception>().WithMessage("DB error");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_Demerito_ReduceBalanceYRecalculaPrivilegios()
    {
        // Arrange
        var alumno = new Alumno("MAT-003", "Pedro", "Ruiz", "1 Primaria");
        // Balance inicial es 0; con config predeterminada, tiene Oficina
        _alumnoRepoMock.Setup(r => r.GetByIdAsync(alumno.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(alumno);
        _configRepoMock.Setup(r => r.GetActivaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_configDefault);

        // Act — aplicar demérito de 3 puntos → balance queda en -3 (umbral comedor es -3, se revoca)
        await _useCase.ExecuteAsync(alumno.Id, Guid.NewGuid(), TipoMerito.Demerito, 3, "Mal comportamiento");

        // Assert
        alumno.BalanceMeritos.Should().Be(-3);
        alumno.PrivilegeStatus.Comedor.Should().BeFalse();
    }
}
