using TalentInstitute.Domain.Exceptions;

namespace TalentInstitute.Domain.Entities;

public class ConfiguracionPrivilegios
{
    public Guid Id { get; private set; }
    public DateTime FechaActualizacion { get; private set; }
    public Guid? StaffIdActualizo { get; private set; }

    public int UmbralOficina { get; private set; }
    public int UmbralOficinaRevocado { get; private set; }
    public int UmbralComedor { get; private set; }
    public int UmbralComedorRevocado { get; private set; }
    public int UmbralPatio { get; private set; }
    public int UmbralPatioRevocado { get; private set; }
    public int UmbralBiblioteca { get; private set; }
    public int UmbralBibliotecaRevocado { get; private set; }
    public int UmbralActividades { get; private set; }
    public int UmbralActividadesRevocado { get; private set; }

    // Required by EF Core
    private ConfiguracionPrivilegios() { }

    public ConfiguracionPrivilegios(
        int umbralOficina = 0, int umbralOficinaRevocado = -2,
        int umbralComedor = 0, int umbralComedorRevocado = -3,
        int umbralPatio = -1, int umbralPatioRevocado = -5,
        int umbralBiblioteca = 3, int umbralBibliotecaRevocado = 0,
        int umbralActividades = 5, int umbralActividadesRevocado = 2,
        Guid? staffIdActualizo = null)
    {
        if (umbralOficinaRevocado >= umbralOficina ||
            umbralComedorRevocado >= umbralComedor ||
            umbralPatioRevocado >= umbralPatio ||
            umbralBibliotecaRevocado >= umbralBiblioteca ||
            umbralActividadesRevocado >= umbralActividades)
        {
            throw new DomainException("Todo umbral de revocación debe ser estrictamente menor que el umbral de otorgamiento para el mismo privilegio.");
        }

        Id = Guid.NewGuid();
        FechaActualizacion = DateTime.UtcNow;
        StaffIdActualizo = staffIdActualizo;
        UmbralOficina = umbralOficina;
        UmbralOficinaRevocado = umbralOficinaRevocado;
        UmbralComedor = umbralComedor;
        UmbralComedorRevocado = umbralComedorRevocado;
        UmbralPatio = umbralPatio;
        UmbralPatioRevocado = umbralPatioRevocado;
        UmbralBiblioteca = umbralBiblioteca;
        UmbralBibliotecaRevocado = umbralBibliotecaRevocado;
        UmbralActividades = umbralActividades;
        UmbralActividadesRevocado = umbralActividadesRevocado;
    }
}
