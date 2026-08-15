# Checklist de Tareas Técnicas (TDD & Clean Architecture)

Plan granular para la base de código .NET.

## 0. Estabilización inmediata detectada en revisión

- [x] Corregir build de API: reemplazar atributos `[PATCH]` inválidos por `[HttpPatch]` en Staff y Méritos.
- [x] Alinear contrato de login inválido: API debe devolver `401 Unauthorized` para que el frontend muestre el flujo esperado.
- [x] Alinear claims JWT con frontend: emitir/leer claim de rol compatible con `AuthService.getRole()`.
- [x] Corregir rutas protegidas del frontend: el perfil de alumno debe permitir acceso a Monitora según la matriz UI/API.
- [x] Completar endpoints de PACEs requeridos por el frontend: catálogo, asignación y consulta por alumno; alinear IDs `Guid` en modelos TypeScript.
- [x] Reemplazar placeholders de Login, Dashboard y Perfil de Alumno por pantallas funcionales conectadas a API.
- [x] Validar manualmente Login, Dashboard y Perfil en navegador con API local/staging.
- [x] Reducir o reajustar presupuestos CSS de componentes Angular; `ng build` pasa sin warnings.
- [x] Implementar módulo real de Entrevistas a Padres; endpoints GET / POST / GET/{id} con casos de uso, repositorio y 4 tests TDD.
- [x] Integrar build Angular, migraciones EF y separación staging/production en GitHub Actions.
- [x] Definir decisión de runtime: **net10.0** en producción. SOW decía .NET 8 pero proyectos ya usan net10.0.
- [x] Limpiar código template (`WeatherForecast`, `Class1`, `UnitTest1`) antes de cierre.

- [x] **1. Arquitectura de la Solución**
  - [x] Crear la solución base: `dotnet new sln -n TalentInstitute`
  - [x] Crear las librerías: `Domain`, `Application`, `Infrastructure`.
  - [x] Crear el proyecto web HTTP: `API` (Web Api vacía - `dotnet new webapi -n TalentInstitute.API`).
  - [x] Establecer referencias mutuas: `API` -> `App` e `Infra`, `Infra` -> `App`, `App` -> `Domain`.

- [x] **2. Proyectos de Testing (xUnit y Moq)**
  - [x] Crear de proyecto de prueba: `TalentInstitute.Domain.Tests`.
  - [x] Crear de proyecto de prueba: `TalentInstitute.Application.Tests`.
  - [x] Instalar librerías de test: `Moq`, `FluentAssertions`, `xunit`.
  - [x] Vincular los proyectos de test a la solución global.

- [x] **3. Implementación: Dominio en TDD**
  - [x] Test Red/Verde: Escribir entidad `EntrevistaPadre` con campos de evaluación de riesgo (violencia, religión, divorcio) y banderas de seguridad.
  - [x] Test Red: Escribir pruebas de fallos pre-programadas para `Alumno` (ej. no tener saldo de medallas negativo).
  - [x] Código Verde: Escribir clase estricta `Alumno.cs` (con `NumeroMatricula`, `Apellido`, `Nivel`, `RowVersion`).
  - [x] Test Red: Escribir pruebas para el comportamiento del ciclo vital del material `Pace`.
  - [x] Código Verde: Programar clase `PACE.cs` y su enlace `AlumnoPace.cs` (con `FechaInicio`, `FechaCompletado`, `PuntajeFinal`, `RowVersion`).
  - [x] Entidad `Staff.cs` + enum `Rol` (Principal, Supervisora, Monitora) con invariantes + 5 tests TDD.
  - [x] Entidad `Meta.cs` + enums `EstadoMeta` / `Turno` — máquina de estados completa (6 estados, 7 transiciones) + 10 tests TDD.
  - [x] Entidad `Merito.cs` + enum `TipoMerito` — invariantes spec-mandated (Puntos > 0, Motivo no vacío, Revocar idempotente) + 5 tests TDD.
  - [x] Invariante `AlumnoPace.ValidarAsignacionUnica()` — no duplicar PACE activo por materia + 3 tests TDD.
  - [x] Entidad `ConfiguracionPrivilegios.cs` — umbrales de privilegios con validación cruzada.

- [x] **4. Implementación: Aplicación (Casos de Uso)**
  - [x] Interfaces: Definir abstracciones de Repositorio (`IPaceRepository`, `IUnitOfWork`, `IAuthService`).
  - [x] Interfaces adicionales: `IStaffRepository`, `IMetaRepository`, `IMeritoRepository`, `IConfiguracionRepository`, `IPasswordHasher`.
  - [x] Uso de Moq para "mentir" a las validaciones y armar el ciclo sin la BD instalada aún.
  - [x] Flujo Funcional: `CheckStudentPaceProgressUseCase` (Anotar el estatus de Score de una Meta).
  - [x] Flujo Funcional: `RegistrarMeritoUseCase` — carga Alumno + Config, crea Merito, recalcula privilegios, guarda con UoW. 4 tests TDD.
  - [x] `LoginUseCase` — reemplazadas credenciales hardcodeadas por `IStaffRepository` + `IPasswordHasher` (BCrypt).
  - [x] Registro global IoC: `DependencyInjection.cs` con todos los use cases registrados.

- [x] **5. Implementación: Infraestructura (Persistencia con EF Core)**
  - [x] Instalar dependencias EF Core: `SqlServer` y `Tools`.
  - [x] Modificar `DbContext` creando los DbSets iniciales + `Staff`, `Metas`, `Meritos`, `ConfiguracionPrivilegios`.
  - [x] Implementar la base `IEntityTypeConfiguration` (Fluent API para migrar el esquema SQL de manera precisa, sin atributos [Table] sucios).
  - [x] Configuraciones EF agregadas: `StaffConfiguration`, `MetaConfiguration`, `MeritoConfiguration`, `ConfiguracionPrivilegiosConfiguration`. Existentes actualizadas: `AlumnoConfiguration`, `AlumnoPaceConfiguration`.
  - [x] Instalar EF Core global y forzar la migración 001_Initial a Azure SQL Local/Nube.
  - [x] Migración `002_AddCoreEntities` — nuevas tablas Staff, Metas, Meritos, ConfiguracionPrivilegios; RowVersion en Alumnos y AlumnoPaces.
  - [x] Crear Repositorios Concretos: `StaffRepository`, `MetaRepository`, `MeritoRepository`, `ConfiguracionRepository`.
  - [x] `BcryptPasswordHasher` — implementa `IPasswordHasher` con BCrypt.Net-Next.

- [x] **6. Ensamblaje: La Capa de API**
  - [x] Declaración de los endpoints principales sin cuerpo. Generación de swagger abierto.
  - [x] Añadir Global Exception Handler (para atrapar `DomainException` y retornar HTTP 400 limpio).
  - [x] Enlazar un endpoint clave a nuestro `UseCase` y comprobar la vida completa (e.g. POST `/api/v1/paces/check`).
  - [x] Test E2E simple (opcional o validado por Swagger manual).

- [x] **7. Seguridad e Implementación (Backend)**
  - [x] Verificar la existencia de la carpeta `/secrets` localmente.
  - [x] Configurar el perfil de publicación de Azure desde el archivo en `/secrets`.
  - [x] Configurar las variables de entorno en el Azure Web App utilizando los valores del Service Principal.
  - [x] Verificar política CORS en `Program.cs`: permitir origen de la SPA en staging y producción.

---

## Fase 3 — Frontend SPA (Angular 18 + TypeScript)

- [x] **8. Setup del Proyecto Frontend**
  - [x] Crear el proyecto: `ng new talentinstitute-web --routing --style=css --strict`
  - [x] Configurar `tsconfig.json` con paths alias (`@app/` → `src/app/`).
  - [x] Crear estructura de módulos/carpetas: `core/` (auth, interceptors, guards), `shared/` (componentes base), `features/` (dashboard, alumno), `models/`.
  - [x] Definir design tokens en `src/styles/tokens.css` (colores, radios, sombras, tipografía); importar en `styles.css` global.
  - [x] Configurar fuente Manrope vía Google Fonts en `index.html`.
  - [x] Registrar `HttpClientModule` en `app.config.ts` y configurar `baseUrl` desde `environment.ts` / `environment.prod.ts`.

- [x] **9. Modelos TypeScript y Servicios API**
  - [x] Definir interfaces en `src/app/models/`: `Alumno`, `Pace`, `AlumnoPace`, `Meta`, `Merito`, `Staff`, `DashboardResumen`, `Alerta`.
  - [x] Crear servicios en `src/app/core/services/`: `AlumnosService`, `PacesService`, `ProgresoService`, `MeritosService`, `DashboardService`, `AuthService`.
  - [x] Cada servicio usa `HttpClient` e inyecta la `baseUrl` desde el environment.
  - [x] **Interceptor JWT** (`JwtInterceptor`): adjunta `Authorization: Bearer <token>` en cada request saliente. Registrar en `app.config.ts`.
  - [x] **Interceptor de errores** (`ErrorInterceptor`): captura errores HTTP globalmente:
    - [x] **401 Unauthorized** → limpiar token de `localStorage` + navegar a `/login`. Cubre dos escenarios: credenciales incorrectas en login *y* token expirado mientras la monitora está en sesión activa en el salón.
    - [x] **403 Forbidden** → toast de advertencia "No tienes permiso para realizar esta acción" sin redirigir.
    - [x] **5xx Server Error** → toast de error genérico con opción de reintentar la última acción.

- [x] **10. Componentes Base (Design System — `SharedModule`)**
  - [x] `ButtonComponent` — variantes de Input: `variant` (primary, secondary, danger, ghost), `loading`, `disabled`.
  - [x] `BadgeComponent` — variante semántica por Input: green, orange, red, blue, gray.
  - [x] `CardComponent` — contenedor base con shadow y border-radius como ng-content wrapper.
  - [x] `ProgressBarComponent` — Input `value` (0–100); clase CSS de color calculada automáticamente por umbral.
  - [x] `ToastService` + `ToastComponent` — servicio inyectable para disparar notificaciones; máximo 3 simultáneas con auto-dismiss.
  - [x] `ModalComponent` — wrapper con `@Output() confirmed` y `@Output() cancelled`; usado para confirmaciones destructivas.
  - [x] `AvatarComponent` — genera iniciales y color de fondo determinístico a partir del nombre del alumno.
  - [x] `SpinnerComponent` — variantes inline y overlay de pantalla completa.

- [x] **11. Autenticación y Guards**
  - [x] `AuthService`: métodos `login()`, `logout()`, `getToken()`, `getRole()`, `isAuthenticated()`. Token guardado en `localStorage`.
  - [x] `AuthGuard` (`CanActivateFn`): redirige a `/login` si no hay token válido.
  - [x] `RoleGuard` (`CanActivateFn`): recibe `data.roles` desde la config de rutas; redirige a `/dashboard` si el rol no tiene acceso.
  - [x] Directiva `*hasRole` (o pipe): oculta elementos del template según el rol del usuario autenticado.

- [x] **12. Módulo de Login**
  - [x] `LoginComponent` con `ReactiveForm`: email, contraseña, selector de rol (visual).
  - [x] Validación en tiempo real: email con formato válido, contraseña mínimo 6 caracteres.
  - [x] Llamada a `POST /api/v1/Auth/login`; en éxito guardar token y navegar según rol.
  - [x] Manejo de error 401: mensaje inline "Credenciales incorrectas" sin recargar la página.
  - [x] Overlay de primera vez (heurística Nielsen #10) al primer login del usuario.

- [x] **13. Módulo Dashboard**
  - [x] `DashboardComponent`: orquesta llamadas paralelas con `forkJoin` a `DashboardService.getResumen()` y `DashboardService.getAlertas()`.
  - [x] `MetricCardComponent`: tarjeta reutilizable para las 4 métricas (alumnos activos, metas hoy, PACEs en revisión, alertas).
  - [x] Banner de alertas funcional inline: muestra alumnos sin meta 2+ días; se oculta si no hay alertas.
  - [x] Tabla de alumnos funcional inline: tabla con paginación, filtro por nivel, navegación al perfil con `routerLink`.
  - [x] `WeeklyChartComponent`: gráfica de barras semanales en CSS puro (sin librería de charts).
  - [x] `ScorePendingListComponent`: panel de PACEs pendientes de Score Station.
  - [x] Ruta protegida: `authGuard` en la ruta; restricción de métricas ejecutivas por rol vía `canViewExecutiveMetrics` en template (Monitora ve lista de alumnos, Principal/Supervisora ven todas las métricas).

- [x] **14. Módulo Alumno (Perfil)**
  - [x] Perfil de alumno funcional inline: obtiene el alumno por `id` desde `ActivatedRoute.params`, carga datos con `AlumnosService`.
  - [x] Hero de alumno funcional inline: avatar, nombre, grado, fecha de ingreso, chips de privilegios activos/inactivos.
  - [x] Tarjetas de PACE funcionales inline con `ProgressBarComponent`, estado y acciones de Score Station.
  - [x] Lista de metas funcional inline separada por turno; toggle con llamada a `PUT /api/v1/Progreso/metas/{metaId}/estatus`.
  - [x] Formulario inline de metas con `ReactiveForm`; llama `POST /api/v1/Progreso`.
  - [x] `QuickActionsComponent`: botones para otorgar mérito, registrar demérito, actualizar puntos — abren `ModalComponent` para confirmar.
  - [x] Log funcional inline de los últimos 10 méritos/deméritos del alumno.
  - [x] Acciones restringidas condicionadas por rol en template.

---

## Fase 4 — Pruebas E2E y Cierre

- [ ] **15. Setup Playwright**
  - [ ] Instalar: `npm init playwright@latest` en el proyecto frontend.
  - [ ] Configurar `playwright.config.ts`: baseURL desde variable de entorno, proyectos para Chromium y WebKit (Safari/iPad).
  - [ ] Crear fixtures de autenticación: helpers que hacen login por rol y almacenan el estado de sesión.

- [ ] **16. Escenarios E2E críticos**
  - [ ] **Flujo Supervisora:** Login → Dashboard carga métricas → navegar a perfil alumno → asignar PACE → registrar puntos → verificar barra de progreso actualizada.
  - [ ] **Flujo Monitora:** Login → Dashboard → seleccionar alumno → marcar meta como completada → otorgar mérito → verificar balance actualizado.
  - [ ] **Flujo Principal:** Login → Dashboard → navegar a gestión de Staff → crear nuevo usuario → verificar en lista.
  - [ ] **Flujo de error:** intentar acción sin autorización (Monitora intentando revocar mérito) → verificar que el elemento no es accesible.
  - [ ] **Flujo de alerta:** simular alumno sin meta 2 días → verificar que aparece en banner de alertas del dashboard.

- [ ] **17. Refinamiento UX final**
  - [ ] Auditoría de las 10 heurísticas de Nielsen contra las pantallas reales en staging.
  - [ ] Prueba de usabilidad en iPad (portrait y landscape): verificar tap targets ≥ 44×44px.
  - [ ] Verificar estados de loading en todas las llamadas API (ninguna pantalla queda en blanco durante carga).
  - [ ] Verificar estados de error en todas las llamadas API (ningún `console.error` sin manejo de UI).
  - [ ] Verificar estados vacíos: dashboard sin alumnos, perfil sin metas del día, sin méritos.

- [ ] **18. Despliegue a Producción**
  - [ ] Crear workflow `deploy-production.yml` en GitHub Actions separado del de staging.
  - [ ] El workflow de producción requiere aprobación manual (GitHub Environment protection rules).
  - [ ] Verificar que las migraciones EF Core se aplican correctamente en la base de datos de producción.
  - [ ] Configurar dominio personalizado y certificado SSL en el Azure Web App.
  - [ ] Verificar Application Insights: telemetría de requests, errores y tiempos de respuesta activa.
  - [ ] Smoke test post-deploy: login con los 3 roles en producción antes de dar por cerrado el despliegue.

- [ ] **19. Documentación de Usuario**
  - [ ] Guía por rol (Principal, Supervisora, Monitora): flujos principales con capturas de pantalla.
  - [ ] Glosario ACE dentro del sistema (accesible desde el menú de ayuda).
  - [ ] Instrucciones de troubleshooting básico (sesión expirada, pantalla en blanco, error de conexión).
