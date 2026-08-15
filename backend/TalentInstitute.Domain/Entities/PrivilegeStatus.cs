namespace TalentInstitute.Domain.Entities;

public record PrivilegeStatus
{
    public bool Oficina { get; init; }
    public bool Comedor { get; init; }
    public bool Patio { get; init; }
    public bool Biblioteca { get; init; }
    public bool Actividades { get; init; }

    public PrivilegeStatus(bool oficina = true, bool comedor = true, bool patio = true, bool biblioteca = false, bool actividades = false)
    {
        Oficina = oficina;
        Comedor = comedor;
        Patio = patio;
        Biblioteca = biblioteca;
        Actividades = actividades;
    }
}
