using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Application.UseCases;
using TalentInstitute.Domain.Entities;
using Xunit;

namespace TalentInstitute.Application.Tests.UseCases;

public class ObtenerCatalogoPacesUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_NormalizaMateriaYMapeaCatalogo()
    {
        // Arrange
        var paces = new List<Pace>
        {
            new("MAT", "1045", 100, 80),
            new("MAT", "1046", 100, 85)
        };

        var paceRepositoryMock = new Mock<IPaceRepository>();
        paceRepositoryMock
            .Setup(r => r.GetCatalogoAsync("MAT", It.IsAny<CancellationToken>()))
            .ReturnsAsync(paces);

        var useCase = new ObtenerCatalogoPacesUseCase(paceRepositoryMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(" mat ");

        // Assert
        result.Should().HaveCount(2);
        result.Select(p => p.NumeroPace).Should().ContainInOrder("1045", "1046");
        result[0].Materia.Should().Be("MAT");
        result[1].PuntajeMinimoAprobacion.Should().Be(85);
    }
}
