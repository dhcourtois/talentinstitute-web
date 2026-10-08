using System;
using FluentAssertions;
using TalentInstitute.Domain.Entities;
using TalentInstitute.Domain.Exceptions;
using Xunit;

namespace TalentInstitute.Domain.Tests.Entities;

public class EntrevistaPadreTests
{
    [Fact]
    public void Constructor_ConDatosValidos_DebeCrearInstancia()
    {
        // Act
        var entrevista = new EntrevistaPadre(
            nombrePadre: "Juan Perez",
            numeroHijos: 2,
            riesgoViolencia: false,
            riesgoDivorcio: false,
            conoceADios: true,
            comentarios: "Padre muy amable",
            aceptado: true
        );

        // Assert
        entrevista.Should().NotBeNull();
        entrevista.NombrePadre.Should().Be("Juan Perez");
        entrevista.NumeroHijos.Should().Be(2);
        entrevista.Aceptado.Should().BeTrue();
    }

    [Fact]
    public void Constructor_NumeroHijosNegativo_DebeLanzarDomainException()
    {
        // Act
        Action action = () => new EntrevistaPadre(
            nombrePadre: "Juan Perez",
            numeroHijos: -1,
            riesgoViolencia: false,
            riesgoDivorcio: false,
            conoceADios: true,
            comentarios: "Prueba"
        );

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage("El número de hijos debe ser mayor o igual a 0.");
    }

    [Fact]
    public void Constructor_NombrePadreVacio_DebeLanzarDomainException()
    {
        // Act
        Action action = () => new EntrevistaPadre(
            nombrePadre: "",
            numeroHijos: 1,
            riesgoViolencia: false,
            riesgoDivorcio: false,
            conoceADios: true,
            comentarios: "Prueba"
        );

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage("El nombre del padre no puede estar vacío.");
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Constructor_RiesgosActivosSinComentarios_DebeLanzarDomainException(bool riesgoViolencia, bool riesgoDivorcio)
    {
        // Act
        Action action = () => new EntrevistaPadre(
            nombrePadre: "Juan Perez",
            numeroHijos: 2,
            riesgoViolencia: riesgoViolencia,
            riesgoDivorcio: riesgoDivorcio,
            conoceADios: false,
            comentarios: "   " // Vacio o espacios
        );

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage("Si hay banderas de riesgo de violencia o divorcio crítico, los comentarios no pueden estar vacíos.");
    }

    // ── Largo del texto ──────────────────────────────────────────────────────
    // Reportado por QA: una nota larga fallaba con error 500. El dominio no
    // miraba el largo y el texto reventaba hasta en la base.

    [Fact]
    public void Constructor_ConComentariosLargosPeroDentroDelTope_NoLanza()
    {
        var texto = new string('a', EntrevistaPadre.LargoMaximoComentarios);

        var acto = () => new EntrevistaPadre("Mamá de prueba", 2, false, false, true, texto);

        acto.Should().NotThrow();
    }

    [Fact]
    public void Constructor_ConComentariosQueExcedenElTope_LanzaDomainExceptionYNoLlegaALaBase()
    {
        var texto = new string('a', EntrevistaPadre.LargoMaximoComentarios + 1);

        var acto = () => new EntrevistaPadre("Mamá de prueba", 2, false, false, true, texto);

        acto.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_ConComentariosLargos_ElMensajeDiceCuantosLleva()
    {
        // Para que quien captura sepa cuánto recortar en lugar de adivinar.
        var texto = new string('a', EntrevistaPadre.LargoMaximoComentarios + 250);

        var acto = () => new EntrevistaPadre("Mamá de prueba", 2, false, false, true, texto);

        acto.Should().Throw<DomainException>()
            .WithMessage($"*{EntrevistaPadre.LargoMaximoComentarios + 250}*");
    }

    [Fact]
    public void Constructor_ConNombreQueExcedeElTope_LanzaDomainException()
    {
        var nombre = new string('a', EntrevistaPadre.LargoMaximoNombre + 1);

        var acto = () => new EntrevistaPadre(nombre, 2, false, false, true, "Comentario");

        acto.Should().Throw<DomainException>();
    }

    [Fact]
    public void LargoMaximoComentarios_DaEspacioParaUnaEntrevistaReal()
    {
        // Mil caracteres eran pocos para recoger contexto familiar, factores de
        // riesgo y seguimientos; es lo que motivó el cambio.
        EntrevistaPadre.LargoMaximoComentarios.Should().BeGreaterThan(1000);
    }
}
