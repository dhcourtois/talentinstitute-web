# Reglas de Dominio y Glosario Canónico — Talent Institute

> Este documento es **la fuente de verdad** para naming, estados, transiciones y reglas de negocio del sistema ACE. Cualquier contradicción entre este documento y otro del spec-kit se resuelve a favor de este.

---

## 1. Glosario Canónico y Convención de Naming

### Regla general de idioma

| Capa | Idioma | Razón |
|---|---|---|
| Lenguaje de negocio (conversación) | Español | El equipo y los usuarios hablan español |
| Entidades de Dominio (.NET) | **Español** | Ubiquitous Language: el código refleja el vocabulario del negocio |
| Tablas SQL (nombres) | **Español** | EF Core mapea directamente; evita una capa de traducción sin valor |
| Columnas SQL | Español o inglés técnico estándar (`Id`, `CreatedAt`) | PascalCase en ambos casos |
| Endpoints REST | **Español** (`/api/v1/Alumnos`, `/api/v1/Meritos`) | Consistencia con las entidades de dominio |
| DTOs / Response models | Español | Misma razón; los campos del JSON reflejan el vocabulario del negocio |
| Código interno (variables, métodos) | Español para conceptos de dominio; inglés para infraestructura (`repository`, `handler`, `middleware`) | Convención estándar .NET + DDD |

### Tabla de traducción canónica

| Término de negocio | Entidad Dominio (.NET) | Tabla SQL | Endpoint REST | Notas |
|---|---|---|---|---|
| Alumno | `Alumno` | `Alumnos` | `/api/v1/Alumnos` | No `Student`, no `Students` |
| Entrevista a Padres | `EntrevistaPadre` | `EntrevistasPadres` | `/api/v1/Entrevistas` | Entrevista inicial con factores de riesgo |
| PACE (módulo) | `Pace` | `Paces` | `/api/v1/Paces` | Acrónimo: sin mayúsculas compuestas |
| PACE asignado a alumno | `AlumnoPace` | `AlumnoPaces` | — (sub-recurso de Alumnos) | No `StudentProgress` |
| Meta diaria | `Meta` | `Metas` | `/api/v1/Progreso/metas` | No `DailyGoal` |
| Mérito o Demérito | `Merito` | `Meritos` | `/api/v1/Meritos` | Sin acento en código: `Merito` |
| Usuario del sistema (personal) | `Staff` | `Staff` | `/api/v1/Staff` | Excepción justificada: término universal en contexto educativo |
| Configuración de privilegios | `ConfiguracionPrivilegios` | `ConfiguracionPrivilegios` | `/api/v1/Configuracion/privilegios` | Ver Sección 4 |

> **Prohibido:** mezclar en el mismo contexto `Alumno` y `Student`, `Meta` y `DailyGoal`, `Merito` y `Merit`. Una vez adoptado el término canónico, es el único que existe en el sistema.

---

## 2. Máquina de Estados — `Meta`

Una `Meta` representa el compromiso diario de un alumno con un bloque de trabajo de un `AlumnoPace`.

```
[Pendiente] ──(alumno trabaja)──► [EnProgreso]
                                       │
                              (monitora verifica)
                                       │
                               ┌───────┴────────┐
                               ▼                ▼
                          [Completada]      [Rechazada]
                               │                │
                    (supervisora score)   (alumno corrige)
                               │                │
                               ▼                │
                           [Scored] ◄───────────┘
                               │
                    (supervisora aprueba)
                               │
                               ▼
                           [Aprobada]  ← estado terminal
```

### Quién puede mover cada transición

| Transición | Actor autorizado | Condición |
|---|---|---|
| `Pendiente → EnProgreso` | Sistema (automático al inicio del turno) | El alumno tiene el PACE activo y hay una meta planificada |
| `EnProgreso → Completada` | Monitora, Supervisora | El alumno reporta haber terminado el bloque asignado |
| `EnProgreso → Rechazada` | Monitora, Supervisora | El trabajo presentado no cumple el mínimo requerido |
| `Rechazada → EnProgreso` | Sistema (automático) | El alumno retoma el trabajo para corrección |
| `Completada → Scored` | Supervisora | Tras validar en Score Station; registra los puntos obtenidos |
| `Scored → Aprobada` | Sistema (automático) | Si el puntaje es ≥ umbral de aprobación del PACE |
| `Scored → Rechazada` | Supervisora | Si el puntaje es < umbral de aprobación |

---

## 3. Máquina de Estados — `AlumnoPace`

Un `AlumnoPace` traza el ciclo de vida completo de un PACE asignado a un alumno.

```
[Asignado] ──(primer meta registrada)──► [EnProgreso]
                                              │
                                   (todas las metas Aprobadas
                                    + puntaje acumulado ≥ umbral)
                                              │
                                              ▼
                                      [ListoParaAutoTest]
                                              │
                              (alumno completa auto-evaluación)
                                              │
                                    ┌─────────┴──────────┐
                                    ▼                    ▼
                              [AutoTestOk]        [AutoTestFallido]
                                    │                    │
                         (supervisora programa)  (alumno repasa y
                          test final)             reintenta autotest)
                                    │                    │
                                    ▼                    │
                             [EnTestFinal] ◄─────────────┘
                                    │
                          ┌─────────┴──────────┐
                          ▼                    ▼
                     [Completado]         [Fallido]
                  (estado terminal)            │
                                      (supervisora decide:
                                       repetir PACE o asignar
                                       PACE de refuerzo)
```

### Reglas adicionales de `AlumnoPace`

- Un alumno puede tener **como máximo un `AlumnoPace` activo por materia** (estados: `Asignado`, `EnProgreso`, `ListoParaAutoTest`, `AutoTestOk`, `EnTestFinal`). No se puede asignar un segundo PACE de la misma materia hasta que el actual esté `Completado` o `Fallido`.
- Un alumno **sí puede tener PACEs activos en múltiples materias simultáneamente** (ej: MAT en progreso y LEC en progreso al mismo tiempo).
- Solo la **Supervisora y el Principal** pueden asignar un nuevo `AlumnoPace`.
- El estado `Fallido` no se considera terminal desde la UI — siempre ofrece la opción de reasignación.

---

## 4. Sistema de Privilegios — Especificación Ejecutable

### Decisión de arquitectura: configurable, no hardcodeado

Los umbrales de privilegios son **configurables por el Principal** y se persisten en la tabla `ConfiguracionPrivilegios`. El método de dominio `Alumno.RecalcularPrivilegios()` recibe un objeto `ConfiguracionPrivilegios` como parámetro — nunca usa constantes internas. Esto garantiza:

- La lógica vive en el Domain (.NET), no en SQL ni en la UI → **100% testeable con TDD**.
- Los umbrales se pueden ajustar por el Principal sin recompilar ni redesplegar el sistema.

### Entidad de dominio `ConfiguracionPrivilegios`

```csharp
// Domain layer — no depende de ninguna otra capa
public record ConfiguracionPrivilegios(
    int UmbralOficina,          // Predeterminado: 0
    int UmbralOficinaRevocado,  // Predeterminado: -2
    int UmbralComedor,          // Predeterminado: 0
    int UmbralComedorRevocado,  // Predeterminado: -3
    int UmbralPatio,            // Predeterminado: -1
    int UmbralPatioRevocado,    // Predeterminado: -5
    int UmbralBiblioteca,       // Predeterminado: 3
    int UmbralBibliotecaRevocado, // Predeterminado: 0
    int UmbralActividades,      // Predeterminado: 5
    int UmbralActividadesRevocado // Predeterminado: 2
);
```

### Firma del método de dominio

```csharp
// En Alumno.cs (Domain)
public void RecalcularPrivilegios(int balanceMeritos, ConfiguracionPrivilegios config)
```

### Flujo del caso de uso al registrar un mérito

1. `RegistrarMeritoUseCase` recibe el comando.
2. Carga el `Alumno` desde `IAlumnoRepository`.
3. Carga la `ConfiguracionPrivilegios` activa desde `IConfiguracionRepository`.
4. Llama `alumno.RecalcularPrivilegios(nuevoBalance, config)`.
5. Persiste el `Merito` y el `Alumno` actualizado mediante `IUnitOfWork.SaveAsync()`.

### Semántica de revocación de un mérito

- Revocar un mérito **recalcula el `PrivilegeStatus` sobre el balance actual resultante**. No se reproducen estados históricos.
- El historial de méritos permanece inmutable como bitácora de auditoría; la revocación agrega un registro de tipo `Revocado`, no modifica el registro original.
- Los privilegios son un **snapshot del estado actual**, no una bitácora histórica. La pregunta "¿qué privilegios tenía el alumno el martes pasado?" no está en el alcance de este sistema.

### Anulación manual por alumno (issue #21)

El balance de méritos sigue siendo la regla de fondo, pero el colegio necesita representar excepciones que un umbral no sabe expresar: una incapacidad médica que impide salir al patio, un permiso puntual del Principal.

`Alumno.PrivilegiosManuales` guarda, por privilegio, un `bool?`:

| Valor | Significado |
|---|---|
| `null` | Automático. Decide el balance contra los umbrales. Es el estado inicial y el de todos los alumnos previos a la migración. |
| `true` | Forzado activo. Se mantiene aunque el balance caiga por debajo del umbral de revocación. |
| `false` | Forzado inactivo. Se mantiene aunque el balance supere el umbral de otorgamiento. |

```csharp
// En Alumno.cs (Domain)
public void EstablecerPrivilegioManual(Privilegio privilegio, bool? valor, ConfiguracionPrivilegios config)
```

La invariante se conserva: el método **no escribe `PrivilegeStatus`**. Registra la excepción y llama a `RecalcularPrivilegios()`, que resuelve cada privilegio como `manual ?? cálculo por umbral`. Devolver un privilegio a `null` lo reevalúa de inmediato contra la configuración vigente.

Restringido al Principal (`PATCH /api/v1/Alumnos/{id}/privilegios`). Forzar un privilegio **no altera el balance de méritos**: es una excepción administrativa, no un premio.

---

## 5. Semana Académica y Turnos

- La **semana académica** va de lunes a viernes. No hay lógica de negocio que opere sobre fines de semana.
- El sistema define **dos turnos por día**: `Mañana` y `Tarde`. Las metas se asocian a un turno al crearlas.
- No existe un corte automático de turno; el cambio es operativo (la monitora/supervisora decide cuándo transicionar manualmente).
- La **semana de referencia** para reportes y alertas se identifica por la fecha de inicio (lunes). El endpoint `GET /Progreso/{alumnoId}/semana/{fechaInicio}` espera una fecha que sea lunes; si no lo es, el sistema devuelve HTTP 400 con mensaje explícito.

---

## 6. Reglas de Alertas

- Un alumno aparece en el banner de alertas cuando lleva **2 o más días hábiles consecutivos** sin ninguna `Meta` en estado `Completada` o superior.
- Las alertas se calculan en tiempo real al consultar el Dashboard (no hay job periódico en esta versión).
- Una alerta se **resuelve automáticamente** en cuanto el alumno registra una meta completada; desaparece del banner sin intervención manual.

---

## 7. Concurrencia

El sistema tiene un volumen máximo de ~3 usuarios simultáneos editando datos de alumnos. El riesgo de conflicto es bajo pero real (ej: Monitora y Supervisora actualizan el mismo alumno al mismo tiempo).

**Estrategia: Concurrencia Optimista con EF Core**

- Las tablas `Alumnos`, `AlumnoPaces` y `Meritos` incluyen una columna `RowVersion` (`ROWVERSION` / `TIMESTAMP` en SQL Server).
- EF Core lanza `DbUpdateConcurrencyException` si dos operaciones intentan modificar el mismo registro.
- El handler de la excepción en la capa de API devuelve **HTTP 409 Conflict** con el mensaje: `"Otro usuario modificó este registro. Por favor recarga y vuelve a intentarlo."`.
- La UI Angular captura el 409 en el `ErrorInterceptor` y muestra un toast con la opción de recargar la vista del alumno afectado.

---

## 8. Invariantes de Dominio (resumen ejecutable)

Estas reglas se validan en el constructor o en métodos del Domain. Su violación lanza `DomainException`.

| Entidad | Invariante |
|---|---|
| `Merito` | `Puntos` debe ser un entero positivo (> 0) |
| `Merito` | `Motivo` no puede ser nulo ni vacío |
| `Merito` | `Tipo` solo acepta los valores `Merito` o `Demerito` |
| `AlumnoPace` | No se puede asignar un PACE si ya existe uno activo para la misma materia |
| `Meta` | `PuntajeObtenido` no puede exceder `PuntajeMaximo` del PACE |
| `Meta` | `Turno` solo acepta `Mañana` o `Tarde` |
| `Alumno` | `PrivilegeStatus` solo se modifica a través de `RecalcularPrivilegios()`, nunca directamente — incluidas las anulaciones manuales, que entran como insumo de ese método |
| `ConfiguracionPrivilegios` | Todo umbral de revocación debe ser estrictamente menor que el umbral de otorgamiento para el mismo privilegio |
| `EntrevistaPadre` | `NumeroHijos` debe ser mayor o igual a 0 |
| `EntrevistaPadre` | Si hay banderas de riesgo de violencia o divorcio crítico, los `Comentarios` no pueden estar vacíos |
