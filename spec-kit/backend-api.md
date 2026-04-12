# Especificación de la API Backend - Talent Institute

Este documento define la arquitectura y los contratos principales del backend, respetando los principios establecidos en `constitution.md`. Todo el desarrollo se realizará siguiendo **Test-Driven Development (TDD)** de forma obligatoria.

## 🏗 Estructura: Arquitectura Limpia (Clean Architecture)

El proyecto backend (.NET 8) se dividirá en cuatro capas principales:

1. **Domain (Core):**
   - El corazón de negocio; no depende de ninguna otra capa.
   - Contiene las interfaces y las entidades de negocio esenciales del sistema ACE:
     - `Alumno`: Representa al estudiante. Contiene el método de dominio `RecalcularPrivilegios(int balanceMeritos, ConfiguracionPrivilegios config)`. Los umbrales son **parametrizados**, no hardcodeados — el objeto `ConfiguracionPrivilegios` se carga desde la base de datos y se pasa como argumento. Esto mantiene la lógica en el Domain (testeable con TDD puro, sin mocks de BD) y los umbrales configurables por el Principal. Ver especificación completa en `domain-rules.md` §4.
     - `ConfiguracionPrivilegios`: Value object / record de dominio que encapsula los umbrales de privilegios. Es inmutable; cada modificación genera un nuevo registro en BD.
     - `Pace`: Material didáctico/módulo de estudio individual asíncrono. Pertenece a una materia y tiene un número único (ej. MAT-1097). *Nota: "PACE" es el término de negocio ACE; `Pace` es el nombre de la clase en código.*
     - `AlumnoPace`: Vínculo relacional que traza el ciclo de vida de un `Pace` asignado a un alumno específico (`Asignado → EnProgreso → ListoParaAutoTest → AutoTestOk/AutoTestFallido → EnTestFinal → Completado/Fallido`). Ver máquina de estados completa en `domain-rules.md` §3.
     - `Meta`: Objetivos diarios/semanales establecidos y cumplidos por el alumno. Vinculada a un `AlumnoPace`.
     - `Merito`: Registro de comportamiento, logros académicos o sanciones disciplinarias. Vinculado al alumno y al staff que lo registró.
     - `Staff`: Miembro del personal del colegio con acceso al sistema. Contiene email, hash de contraseña y rol asignado (Principal / Supervisora / Monitora).

2. **Application (Use Cases):**
   - Depende exclusivamente del `Domain`.
   - Implementa la lógica de la aplicación y orquesta los casos de uso principales.
   - Define los contratos (interfaces) para repositorios y servicios externos (IRepository, IEmailService, etc.).

3. **Infrastructure:**
   - Depende de `Application`.
   - Implementa los contratos definidos en la capa de aplicación. Incluye repositorios en Entity Framework Core conectándose a **Azure SQL**, almacenamiento y servicios externos.

4. **API (Presentation):**
   - El punto de entrada web que depende de `Application` e, indirectamente mediante Inyección de Dependencias, de `Infrastructure`.
   - Contiene controladores HTTP, configuraciones de Swagger, middlewares, filtros de validación y manejo global de excepciones.

## 🔐 Seguridad y Roles

### Autenticación y Autorización
- La autenticación se maneja globalmente y sin estado a través de **JSON Web Tokens (JWT)**.
- Para proteger los endpoints se utilizarán **Políticas de Acceso por Claims** integradas en ASP.NET Core (`[Authorize(Policy = "...")]`).

### Privilegios por Rol
1. **Principal:** Acceso administrativo total. Gestión de usuarios de staff, configuración de privilegios, y acceso al Dashboard General.
2. **Supervisora:** Autoridad académica del aula principal. Encargada de la asignación de PACES, validación y aprobación de metas estratégicas y evaluación del desempeño.
3. **Monitora:** Personal de apoyo en el aula. Funciones de revisión de avances diarios, calificación operativa (scoring), registro de asistencia y asignación directa de méritos o deméritos rápidos.

## 📡 Endpoints Principales (REST)

Se utilizará como base el ruteo `/api/v1/`.

### 1. Autenticación (`/api/v1/Auth`)
- `POST /login` — Autentica al staff, devuelve JWT con claims de identidad y rol.

### 2. Gestión de Alumnos (`/api/v1/Alumnos`)
- `GET /` — Lista todos los alumnos activos. Soporta paginación y filtro por nivel. (Acceso: todos los roles.)
- `GET /{alumnoId}` — Perfil completo del alumno: datos personales, PACEs activos, balance de méritos, privilegios. (Acceso: todos los roles.)
- `POST /` — Da de alta un nuevo alumno. (Acceso: Principal, Supervisora.)
- `PUT /{alumnoId}` — Actualiza datos generales del alumno. (Acceso: Principal, Supervisora.)
- ~~`PATCH /{alumnoId}/privilegios`~~ — **Este endpoint no existe.** `PrivilegioStatus` es de solo lectura para la API; únicamente se modifica internamente vía `Alumno.RecalcularPrivilegios()` al registrar o revocar un mérito. Ver invariante en `domain-rules.md` §8.

### 3. Catálogo de PACEs (`/api/v1/Paces`)
- `GET /` — Lista el catálogo de PACEs disponibles, filtrable por materia (`?subject=MAT`). (Acceso: todos los roles.)
- `POST /` — Agrega un nuevo PACE al catálogo institucional. (Acceso: Principal.)
- `POST /asignar` — Asigna un Pace del catálogo a un alumno específico, creando el registro `AlumnoPace`. Body: `{ alumnoId, paceId }`. (Acceso: Supervisora, Principal.)
- `GET /alumno/{alumnoId}` — Obtiene todos los PACEs asignados a un alumno con su estado de progreso actual. (Acceso: todos los roles.)

### 4. Registro de Avances y Metas (`/api/v1/Progreso`)
- `POST /` — Registra o aprueba el avance semanal en el panel de metas de un alumno.
- `GET /{alumnoId}/semana/{fechaInicio}` — Recupera el estado de las metas de un alumno en una semana específica.
- `PUT /metas/{metaId}/estatus` — Avanza el estado de una `Meta` según la máquina canónica de `domain-rules.md` §2. El body incluye `{ "estado": "<nuevoEstado>" }`. Transiciones permitidas y quién las autoriza:
  - `EnProgreso → Completada` — Monitora, Supervisora
  - `EnProgreso → Rechazada` — Monitora, Supervisora
  - `Completada → Scored` (+ campo `puntajeObtenido`) — Supervisora
  - `Scored → Aprobada` — Sistema (automático si `puntajeObtenido ≥ puntajeMinimo`)
  - `Scored → Rechazada` — Supervisora (si `puntajeObtenido < puntajeMinimo`)
  - Cualquier transición no listada devuelve HTTP 422 con mensaje explícito de la transición inválida.

### 5. Gestión de Méritos y Deméritos (`/api/v1/Meritos`)
- `POST /` — Registra un nuevo evento de mérito o demérito. (Acceso: Supervisora, Monitora.)
- `GET /alumno/{alumnoId}` — Balance actual e historial cronológico de comportamiento.
- `PATCH /{meritoId}/revocar` — Revoca un registro mal ingresado. (Acceso: Principal, Supervisora.)

### 6. Gestión de Staff (`/api/v1/Staff`)
- `GET /` — Lista todos los usuarios del sistema con su rol. (Acceso: Principal.)
- `POST /` — Crea un nuevo usuario de staff. Body: `{ email, password, role }`. (Acceso: Principal.)
- `PUT /{staffId}` — Actualiza datos o rol de un usuario. (Acceso: Principal.)
- `PATCH /{staffId}/desactivar` — Desactiva el acceso de un usuario sin eliminarlo. (Acceso: Principal.)

### 7. Dashboard (`/api/v1/Dashboard`)
- `GET /resumen` — Devuelve las métricas del dashboard general: alumnos activos, metas cumplidas hoy, PACEs en revisión, conteo de alertas. (Acceso: Principal, Supervisora.)
- `GET /alertas` — Lista de alumnos con 2 o más días sin registrar meta, ordenados por días de inactividad. (Acceso: Principal, Supervisora.)

## 🧪 Especificación de Pruebas TDD

El Testing no es algo implementado a posteriori; es quien rige el ciclo de software en 'Talent Institute'.

- **Herramientas Principales:** `xUnit` como orquestador de pruebas y afirmaciones, y `Moq` para el aislamiento de dependencias.
- **Métricas:** Es obligatorio apuntar y mantener un **>90% de cobertura (Code Coverage)** en la vital capa de `Application` (Casos de uso). El código no llegará a la rama principal (main) si rompe esta regla.
- **Estrategia General (Red - Green - Refactor):**
  - **Dominio:** Pruebas de comportamiento puro sobre el estado interno de las entidades. Sin mocking. Todos los tests de `RecalcularPrivilegios` reciben una instancia de `ConfiguracionPrivilegios` con los valores que el test necesita — no dependen de configuración externa. Casos obligatorios:
    - `dado_balance_cero_con_config_predeterminada_privilegios_basicos_activos` — Oficina, Comedor y Patio activos.
    - `dado_balance_por_debajo_de_umbral_revocacion_oficina_se_revoca`.
    - `dado_balance_por_debajo_de_umbral_revocacion_patio_se_revoca`.
    - `dado_balance_igual_o_superior_a_umbral_biblioteca_se_activa`.
    - `dado_balance_igual_o_superior_a_umbral_actividades_especiales_se_activan`.
    - `dado_config_personalizada_los_umbrales_personalizados_se_respetan` — verifica que la lógica usa el `config` recibido, no constantes internas.
    - `no_se_puede_crear_merito_con_puntos_cero_o_negativos` — lanza `DomainException`.
    - `no_se_puede_crear_merito_sin_motivo` — lanza `DomainException`.
    - `no_se_puede_asignar_pace_si_ya_existe_uno_activo_para_la_misma_materia` — lanza `DomainException`.
  - **Application (Use Cases):** Validar orquestación con `Moq`. Casos obligatorios:
    - Al registrar un mérito válido, se llama a `IMeritoRepository.SaveAsync()` **y** a `IAlumnoRepository.UpdateAsync()` con el `PrivilegeStatus` recalculado.
    - Si el repositorio lanza una excepción, el caso de uso propaga el error correctamente sin estado parcial guardado.
