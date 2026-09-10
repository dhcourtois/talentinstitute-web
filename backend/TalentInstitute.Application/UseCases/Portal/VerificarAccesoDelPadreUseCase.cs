using System;
using System.Threading;
using System.Threading.Tasks;
using TalentInstitute.Application.Interfaces;
using TalentInstitute.Domain.Exceptions;

namespace TalentInstitute.Application.UseCases.Portal;

/// <summary>
/// Comprueba que un alumno esté vinculado al padre autenticado (issue #8).
///
/// Es la pieza que separa a un padre del expediente de un hijo ajeno. Todo
/// endpoint del portal que reciba un `alumnoId` debe pasar por aquí antes de
/// leer cualquier dato: el id viaja en la URL y el padre puede cambiarlo.
/// </summary>
public class VerificarAccesoDelPadreUseCase
{
    private readonly IPadreFamiliaRepository _padreFamiliaRepository;

    public VerificarAccesoDelPadreUseCase(IPadreFamiliaRepository padreFamiliaRepository)
    {
        _padreFamiliaRepository = padreFamiliaRepository;
    }

    public async Task EnsureAsync(Guid padreFamiliaId, Guid alumnoId, CancellationToken cancellationToken = default)
    {
        var padre = await _padreFamiliaRepository.GetByIdAsync(padreFamiliaId, cancellationToken);

        // Una cuenta desactivada conserva sus vínculos, así que hay que mirar el
        // estado y no solo la relación.
        if (padre is null || !padre.Activo)
            throw new AccesoDenegadoException();

        if (!await _padreFamiliaRepository.TieneAlumnoAsync(padreFamiliaId, alumnoId, cancellationToken))
            throw new AccesoDenegadoException();
    }
}

/// <summary>
/// El mensaje es deliberadamente igual exista o no el alumno: distinguir
/// "no existe" de "no es tu hijo" permitiría sondear la matrícula del colegio.
/// </summary>
public class AccesoDenegadoException : DomainException
{
    public AccesoDenegadoException()
        : base("No tienes acceso a la información de este alumno.")
    {
    }
}
