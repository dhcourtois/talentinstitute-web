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
    private readonly Mock<IStaffRepository>         _repoMock   = new();
    private readonly Mock<IPadreFamiliaRepository>  _padreMock  = new();
    private readonly Mock<IPasswordHasher>          _hasherMock = new();
    private readonly Mock<IJwtProvider>             _jwtMock    = new();

    private LoginUseCase CreateSut() =>
        new(_repoMock.Object, _padreMock.Object, _hasherMock.Object, _jwtMock.Object);

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

    // Un cliente que va una versión atrás no manda vistaInicial. Ese caso debe
    // autenticar igual: la vista inicial es preferencia de presentación, no un
    // control de acceso, y rechazarla dejaba el login inservible ante cualquier
    // desfase de versión entre frontend y backend.
    [Theory]
    [InlineData(Rol.Supervisora, "")]
    [InlineData(Rol.Monitora,    "   ")]
    [InlineData(Rol.Principal,   null)]
    public async Task ExecuteAsync_VistaInicialVacia_DebeRetornarToken(Rol rol, string? vistaInicial)
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
        var result = await sut.ExecuteAsync("test@talentinstitute.com", "password", vistaInicial!);

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

    // ── Acceso de padres de familia (issue #8) ───────────────────────────────

    private PadreFamilia CrearPadre() => new("papa@ejemplo.com", "hashedPassword", "Juan Pérez");

    [Fact]
    public async Task ExecuteAsync_SinCuentaDePersonal_AutenticaComoPadre()
    {
        // Arrange: una sola pantalla de acceso para dos tablas distintas.
        _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Staff?)null);
        _padreMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(CrearPadre());
        _hasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _jwtMock.Setup(j => j.Generate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("token-padre");

        // Act
        var result = await CreateSut().ExecuteAsync("papa@ejemplo.com", "password", "");

        // Assert
        result.Should().Be("token-padre");
    }

    [Fact]
    public async Task ExecuteAsync_ComoPadre_EmiteElTokenConRolPadre()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Staff?)null);
        _padreMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(CrearPadre());
        _hasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _jwtMock.Setup(j => j.Generate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("token");

        // Act
        await CreateSut().ExecuteAsync("papa@ejemplo.com", "password", "");

        // Assert: el rol del token es lo que autoriza cada endpoint del portal.
        _jwtMock.Verify(j => j.Generate(It.IsAny<string>(), It.IsAny<string>(), PadreFamilia.RolToken), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_PadreDesactivado_NoPermiteEntrar()
    {
        // Arrange
        var padre = CrearPadre();
        padre.Desactivar();

        _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Staff?)null);
        _padreMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(padre);
        _hasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        // Act
        var acto = async () => await CreateSut().ExecuteAsync("papa@ejemplo.com", "password", "");

        // Assert
        await acto.Should().ThrowAsync<Exception>().WithMessage("Credenciales inválidas.");
    }

    [Fact]
    public async Task ExecuteAsync_PadreConCredencialIncorrecta_NoPermiteEntrar()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Staff?)null);
        _padreMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(CrearPadre());
        _hasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        // Act
        var acto = async () => await CreateSut().ExecuteAsync("papa@ejemplo.com", "mala", "");

        // Assert
        await acto.Should().ThrowAsync<Exception>().WithMessage("Credenciales inválidas.");
    }

    [Fact]
    public async Task ExecuteAsync_CorreoInexistenteEnAmbasTablas_NoRevelaCualFalta()
    {
        // Arrange: el mismo mensaje en los dos casos impide sondear qué cuentas existen.
        _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Staff?)null);
        _padreMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync((PadreFamilia?)null);

        // Act
        var acto = async () => await CreateSut().ExecuteAsync("nadie@ejemplo.com", "password", "");

        // Assert
        await acto.Should().ThrowAsync<Exception>().WithMessage("Credenciales inválidas.");
    }
}
