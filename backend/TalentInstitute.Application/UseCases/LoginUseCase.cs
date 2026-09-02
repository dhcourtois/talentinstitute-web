using TalentInstitute.Application.Interfaces;

namespace TalentInstitute.Application.UseCases;

public class LoginUseCase
{
    private readonly IStaffRepository _staffRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;

    public LoginUseCase(IStaffRepository staffRepository, IPasswordHasher passwordHasher, IJwtProvider jwtProvider)
    {
        _staffRepository = staffRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
    }

    public async Task<string> ExecuteAsync(string email, string password, string vistaInicial, CancellationToken cancellationToken = default)
    {
        var staff = await _staffRepository.GetByEmailAsync(email, cancellationToken)
            ?? throw new Exception("Credenciales inválidas.");

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
}
