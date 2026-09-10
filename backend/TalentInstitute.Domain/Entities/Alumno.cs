using TalentInstitute.Domain.Exceptions;

namespace TalentInstitute.Domain.Entities;

public class Alumno
{
    public Guid Id { get; private set; }
    public string NumeroMatricula { get; private set; }
    public string Nombre { get; private set; }
    public string Apellido { get; private set; }
    public string Nivel { get; private set; }
    public DateTime FechaIngreso { get; private set; }
    public int BalanceMeritos { get; private set; }
    public PrivilegeStatus PrivilegeStatus { get; private set; }
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    /// <param name="fechaIngreso">
    /// Opcional. Si se omite se toma la fecha de alta, que es el comportamiento
    /// histórico; se recibe para poder capturar alumnos que ya estaban en el
    /// colegio antes de que existiera el sistema.
    /// </param>
    public Alumno(string numeroMatricula, string nombre, string apellido, string nivel, DateTime? fechaIngreso = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre del alumno no puede estar vacío.");

        if (string.IsNullOrWhiteSpace(apellido))
            throw new DomainException("El apellido del alumno no puede estar vacío.");

        if (string.IsNullOrWhiteSpace(nivel))
            throw new DomainException("El nivel del alumno no puede estar vacío.");

        Id = Guid.NewGuid();
        NumeroMatricula = numeroMatricula;
        Nombre = nombre;
        Apellido = apellido;
        Nivel = nivel;
        FechaIngreso = NormalizarFechaIngreso(fechaIngreso ?? DateTime.UtcNow);
        BalanceMeritos = 0;
        PrivilegeStatus = new PrivilegeStatus();
    }

    /// <summary>
    /// Actualiza los datos generales del alumno. La matrícula no se edita:
    /// identifica al alumno y cambiarla rompería su historial.
    /// </summary>
    /// <param name="fechaIngreso">
    /// Opcional. Si se omite, la fecha de ingreso queda como estaba: quien edite
    /// solo el nivel no debería mover una fecha que no tocó.
    /// </param>
    public void ActualizarDatos(string nombre, string apellido, string nivel, DateTime? fechaIngreso = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre del alumno no puede estar vacío.");

        if (string.IsNullOrWhiteSpace(apellido))
            throw new DomainException("El apellido del alumno no puede estar vacío.");

        if (string.IsNullOrWhiteSpace(nivel))
            throw new DomainException("El nivel del alumno no puede estar vacío.");

        Nombre = nombre;
        Apellido = apellido;
        Nivel = nivel;

        if (fechaIngreso.HasValue)
            FechaIngreso = NormalizarFechaIngreso(fechaIngreso.Value);
    }

    /// <summary>
    /// La fecha de ingreso es un dato de calendario, no un instante: se guarda a
    /// medianoche para que dos alumnos dados de alta el mismo día comparen igual.
    /// </summary>
    private static DateTime NormalizarFechaIngreso(DateTime fecha)
    {
        if (fecha == default)
            throw new DomainException("La fecha de ingreso del alumno no es válida.");

        if (fecha.Year < 1900)
            throw new DomainException("La fecha de ingreso del alumno no puede ser anterior al año 1900.");

        return fecha.Date;
    }

    public void RecalcularPrivilegios(int nuevoBalance, ConfiguracionPrivilegios config)
    {
        BalanceMeritos = nuevoBalance;

        bool oficina = CalcularEstadoPrivilegio(PrivilegeStatus.Oficina, nuevoBalance, config.UmbralOficina, config.UmbralOficinaRevocado);
        bool comedor = CalcularEstadoPrivilegio(PrivilegeStatus.Comedor, nuevoBalance, config.UmbralComedor, config.UmbralComedorRevocado);
        bool patio = CalcularEstadoPrivilegio(PrivilegeStatus.Patio, nuevoBalance, config.UmbralPatio, config.UmbralPatioRevocado);
        bool biblioteca = CalcularEstadoPrivilegio(PrivilegeStatus.Biblioteca, nuevoBalance, config.UmbralBiblioteca, config.UmbralBibliotecaRevocado);
        bool actividades = CalcularEstadoPrivilegio(PrivilegeStatus.Actividades, nuevoBalance, config.UmbralActividades, config.UmbralActividadesRevocado);

        PrivilegeStatus = new PrivilegeStatus(oficina, comedor, patio, biblioteca, actividades);
    }

    private bool CalcularEstadoPrivilegio(bool estadoActual, int balance, int umbralOtorgamiento, int umbralRevocacion)
    {
        if (estadoActual)
        {
            // Si lo tiene, lo pierde si cae por debajo o igual al umbral de revocacion
            if (balance <= umbralRevocacion)
                return false;
            return true;
        }
        else
        {
            // Si no lo tiene, lo gana si alcanza el umbral de otorgamiento
            if (balance >= umbralOtorgamiento)
                return true;
            return false;
        }
    }
}
