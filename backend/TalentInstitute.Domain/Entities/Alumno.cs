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

    /// <summary>
    /// Excepciones manuales por privilegio. Vacío = todo automático, que es como
    /// nace y como se comportaba el sistema antes del issue #21.
    /// </summary>
    public PrivilegiosManuales PrivilegiosManuales { get; private set; }

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    /// <summary>
    /// Requerido por EF Core. Al volverse opcional la fecha de ingreso, el
    /// constructor público dejó de poder enlazarse a las propiedades mapeadas
    /// (`DateTime?` contra `DateTime`), así que la materialización necesita
    /// esta puerta. EF sobrescribe todo lo que se asigne aquí.
    /// </summary>
    private Alumno()
    {
        NumeroMatricula = string.Empty;
        Nombre = string.Empty;
        Apellido = string.Empty;
        Nivel = string.Empty;
        PrivilegeStatus = new PrivilegeStatus();
        PrivilegiosManuales = new PrivilegiosManuales();
    }

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
        PrivilegiosManuales = new PrivilegiosManuales();
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

        bool oficina = ResolverPrivilegio(PrivilegiosManuales.Oficina, PrivilegeStatus.Oficina, nuevoBalance, config.UmbralOficina, config.UmbralOficinaRevocado);
        bool comedor = ResolverPrivilegio(PrivilegiosManuales.Comedor, PrivilegeStatus.Comedor, nuevoBalance, config.UmbralComedor, config.UmbralComedorRevocado);
        bool patio = ResolverPrivilegio(PrivilegiosManuales.Patio, PrivilegeStatus.Patio, nuevoBalance, config.UmbralPatio, config.UmbralPatioRevocado);
        bool biblioteca = ResolverPrivilegio(PrivilegiosManuales.Biblioteca, PrivilegeStatus.Biblioteca, nuevoBalance, config.UmbralBiblioteca, config.UmbralBibliotecaRevocado);
        bool actividades = ResolverPrivilegio(PrivilegiosManuales.Actividades, PrivilegeStatus.Actividades, nuevoBalance, config.UmbralActividades, config.UmbralActividadesRevocado);

        PrivilegeStatus = new PrivilegeStatus(oficina, comedor, patio, biblioteca, actividades);
    }

    /// <summary>
    /// Fuerza un privilegio a activo o inactivo, o lo devuelve a automático con
    /// <paramref name="valor"/> nulo (issue #21).
    ///
    /// No escribe `PrivilegeStatus` directamente: registra la excepción y deja
    /// que `RecalcularPrivilegios` la aplique. Así se conserva la invariante de
    /// que el estado de privilegios tiene una sola puerta de entrada.
    /// </summary>
    public void EstablecerPrivilegioManual(Privilegio privilegio, bool? valor, ConfiguracionPrivilegios config)
    {
        PrivilegiosManuales = PrivilegiosManuales.Con(privilegio, valor);
        RecalcularPrivilegios(BalanceMeritos, config);
    }

    /// <summary>
    /// Una anulación manual gana sobre el umbral; en automático decide el balance.
    /// </summary>
    private static bool ResolverPrivilegio(bool? manual, bool estadoActual, int balance, int umbralOtorgamiento, int umbralRevocacion)
        => manual ?? CalcularEstadoPrivilegio(estadoActual, balance, umbralOtorgamiento, umbralRevocacion);

    private static bool CalcularEstadoPrivilegio(bool estadoActual, int balance, int umbralOtorgamiento, int umbralRevocacion)
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
