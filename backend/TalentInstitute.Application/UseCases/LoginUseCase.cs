using TalentInstitute.Application.Interfaces;
using TalentInstitute.Domain.Entities;

namespace TalentInstitute.Application.UseCases;

public class LoginUseCase
{
    private readonly IStaffRepository _staffRepository;
    private readonly IPadreFamiliaRepository _padreFamiliaRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;

    public LoginUseCase(
        IStaffRepository staffRepository,
        IPadreFamiliaRepository padreFamiliaRepository,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider)
    {
        _staffRepository = staffRepository;
        _padreFamiliaRepository = padreFamiliaRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
    }

    public async Task<string> ExecuteAsync(string email, string password, string vistaInicial, CancellationToken cancellationToken = default)
    {
        var staff = await _staffRepository.GetByEmailAsync(email, cancellationToken);

        // Sin cuenta de personal se intenta como padre de familia (issue #8).
        // Son dos tablas distintas y una sola pantalla de acceso, así que quien
        // entra no tiene que saber de antemano en cuál está dado de alta.
        if (staff is null)
            return await AutenticarPadreAsync(email, password, vistaInicial, cancellationToken);

        if (!staff.Activo)
            throw new Exception("Credenciales inválidas.");

        if (!_passwordHasher.Verify(password, staff.PasswordHash))
            throw new Exception("Credenciales inválidas.");

        // Una vistaInicial vacía significa "sin preferencia": el cliente no la
        // envió. Rechazar ese caso rompe el login por completo cuando el
        // frontend va una versión atrás del backend, y la vista inicial es una
        // preferencia de presentación, no un control de acceso: el rol real
        // viaja en el token y es lo que autoriza cada endpoint.
        if (!string.IsNullOrWhiteSpace(vistaInicial)
            && !string.Equals(staff.Rol.ToString(), vistaInicial, StringComparison.OrdinalIgnoreCase))
            throw new Exception($"La Vista Inicial seleccionada no corresponde al rol del usuario. Tu usuario tiene el rol de {staff.Rol}.");

        return _jwtProvider.Generate(staff.Id.ToString(), staff.Email, staff.Rol.ToString());
    }

    private async Task<string> AutenticarPadreAsync(
        string email,
        string password,
        string vistaInicial,
        CancellationToken cancellationToken)
    {
        var padre = await _padreFamiliaRepository.GetByEmailAsync(email, cancellationToken)
            ?? throw new Exception("Credenciales inválidas.");

        if (!padre.Activo)
            throw new Exception("Credenciales inválidas.");

        if (!_passwordHasher.Verify(password, padre.PasswordHash))
            throw new Exception("Credenciales inválidas.");

        // Misma tolerancia que con el personal: la vista inicial es una
        // preferencia de presentación, no un control de acceso.
        if (!string.IsNullOrWhiteSpace(vistaInicial)
            && !string.Equals(PadreFamilia.RolToken, vistaInicial, StringComparison.OrdinalIgnoreCase))
            throw new Exception("La Vista Inicial seleccionada no corresponde al rol del usuario. Tu usuario es de padre de familia.");

        return _jwtProvider.Generate(padre.Id.ToString(), padre.Email, PadreFamilia.RolToken);
    }
}
