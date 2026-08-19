using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Application.UseCases;
using TalentInstitute.Domain.Entities;
using Xunit;

namespace TalentInstitute.Application.Tests.UseCases;

public class LoginUseCaseTests
{
    private readonly Mock<IStaffRepository> _repoMock     = new();
    private readonly Mock<IPasswordHasher>  _hasherMock   = new();
    private readonly Mock<IJwtProvider>     _jwtMock      = new();

    private LoginUseCase CreateSut() =>
        new(_repoMock.Object, _hasherMock.Object, _jwtMock.Object);

    private Staff CreateStaff(Rol rol) =>
        new("test@talentinstitute.com", "hashedPassword", rol);

    [Theory]
    [InlineData(Rol.Supervisora, "Supervisora")]
    [InlineData(Rol.Monitora,    "Monitora")]
    [InlineData(Rol.Principal,   "Principal")]
    public async Task ExecuteAsync_VistaInicialMatchesRol_DebeRetornarToken(Rol rol, string vistaInicial)
    {
        // Arrange
        var staff = CreateStaff(rol);
        _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(staff);
        _hasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _jwtMock.Setup(j => j.Generate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("token");

        var sut = CreateSut();

        // Act
        var result = await sut.ExecuteAsync("test@talentinstitute.com", "password", vistaInicial);

        // Assert
        result.Should().Be("token");
    }

    [Theory]
    [InlineData(Rol.Supervisora, "Monitora")]
    [InlineData(Rol.Supervisora, "Principal")]
    [InlineData(Rol.Monitora,    "Supervisora")]
    [InlineData(Rol.Monitora,    "Principal")]
    [InlineData(Rol.Principal,   "Supervisora")]
    [InlineData(Rol.Principal,   "Monitora")]
    public async Task ExecuteAsync_VistaInicialNoMatchRol_DebeLanzarExcepcion(Rol rol, string vistaInicial)
    {
        // Arrange
        var staff = CreateStaff(rol);
        _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(staff);
        _hasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        var sut = CreateSut();

        // Act
        var act = () => sut.ExecuteAsync("test@talentinstitute.com", "password", vistaInicial);

        // Assert
        await act.Should().ThrowAsync<Exception>()
                 .WithMessage("*Vista Inicial*");
    }

    [Fact]
    public async Task ExecuteAsync_UsuarioNoExiste_DebeLanzarExcepcion()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Staff?)null);

        var sut = CreateSut();

        // Act
        var act = () => sut.ExecuteAsync("noexiste@talentinstitute.com", "password", "Supervisora");

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Credenciales inválidas.");
    }

    [Fact]
    public async Task ExecuteAsync_ContrasenaIncorrecta_DebeLanzarExcepcion()
    {
        // Arrange
        var staff = CreateStaff(Rol.Supervisora);
        _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(staff);
        _hasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var sut = CreateSut();

        // Act
        var act = () => sut.ExecuteAsync("test@talentinstitute.com", "wrong", "Supervisora");

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Credenciales inválidas.");
    }
}
