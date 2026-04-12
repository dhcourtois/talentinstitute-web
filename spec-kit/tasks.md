# Checklist de Tareas Técnicas (TDD & Clean Architecture)

Plan granular para la base de código .NET.

- [ ] **1. Arquitectura de la Solución**
  - [ ] Crear la solución base: `dotnet new sln -n TalentInstitute`
  - [ ] Crear las librerías: `Domain`, `Application`, `Infrastructure`.
  - [ ] Crear el proyecto web HTTP: `API` (Web Api vacía - `dotnet new webapi -n TalentInstitute.API`).
  - [ ] Establecer referencias mutuas: `API` -> `App` e `Infra`, `Infra` -> `App`, `App` -> `Domain`.

- [ ] **2. Proyectos de Testing (xUnit y Moq)**
  - [ ] Crear de proyecto de prueba: `TalentInstitute.Domain.Tests`.
  - [ ] Crear de proyecto de prueba: `TalentInstitute.Application.Tests`.
  - [ ] Instalar librerías de test: `Moq`, `FluentAssertions`, `xunit`.
  - [ ] Vincular los proyectos de test a la solución global.

- [ ] **3. Implementación: Dominio en TDD**
  - [ ] Test Red: Escribir pruebas de fallos pre-programadas para `Alumno` (ej. no tener saldo de medallas negativo).
  - [ ] Código Verde: Escribir clase estricta `Alumno.cs`.
  - [ ] Test Red: Escribir pruebas para el comportamiento del ciclo vital del material `Pace`.
  - [ ] Código Verde: Programar clase `PACE.cs` y su enlace `AlumnoPace.cs`.

- [ ] **4. Implementación: Aplicación (Casos de Uso)**
  - [ ] Interfaces: Definir abstracciones de Repositorio (`IPaceRepository`, `IUnitOfWork`, `IAuthService`).
  - [ ] Uso de Moq para "mentir" a las validaciones y armar el ciclo sin la BD instalada aún.
  - [ ] Flujo Funcional: `CheckStudentPaceProgressUseCase` (Anotar el estatus de Score de una Meta).
  - [ ] Registro global IoC: Crear `DependencyInjection.cs` propio interno del proyecto App.

- [ ] **5. Implementación: Infraestructura (Persistencia con EF Core)**
  - [ ] Instalar dependencias EF Core: `SqlServer` y `Tools`.
  - [ ] Modificar `DbContext` creando los DbSets iniciales.
  - [ ] Implementar la base `IEntityTypeConfiguration` (Fluent API para migrar el esquema SQL de manera precisa, sin atributos [Table] sucios).
  - [ ] Instalar EF Core global y forzar la migración 001_Initial a Azure SQL Local/Nube.
  - [ ] Crear Repositorios Concretos.

- [ ] **6. Ensamblaje: La Capa de API**
  - [ ] Declaración de los endpoints principales sin cuerpo. Generación de swagger abierto.
  - [ ] Configuración nativa del middleware de JWT: `services.AddAuthentication(JwtBearerDefaults...)`.
  - [ ] Mapear los roles a Autorización (`[Authorize(Roles = "Supervisora")]`).
  - [ ] Inyección de Middleware global de Excepciones del Dominio para devolver Error HTTP 400 automático con la razón en JSON si el Dominio es corrompido.

- [ ] **7. Seguridad e Implementación (Backend)**
  - [ ] Verificar la existencia de la carpeta `/secrets` localmente.
  - [ ] Configurar el perfil de publicación de Azure desde el archivo en `/secrets`.
  - [ ] Configurar las variables de entorno en el Azure Web App utilizando los valores del Service Principal.
  - [ ] Verificar política CORS en `Program.cs`: permitir origen de la SPA en staging y producción.

---

## Fase 3 — Frontend SPA (Angular 18 + TypeScript)

- [ ] **8. Setup del Proyecto Frontend**
  - [ ] Crear el proyecto: `ng new talentinstitute-web --routing --style=css --strict`
  - [ ] Configurar `tsconfig.json` con paths alias (`@app/` → `src/app/`).
  - [ ] Crear estructura de módulos/carpetas: `core/` (auth, interceptors, guards), `shared/` (componentes base), `features/` (dashboard, alumno), `models/`.
  - [ ] Definir design tokens en `src/styles/tokens.css` (colores, radios, sombras, tipografía); importar en `styles.css` global.
  - [ ] Configurar fuente Manrope vía Google Fonts en `index.html`.
  - [ ] Registrar `HttpClientModule` en `app.config.ts` y configurar `baseUrl` desde `environment.ts` / `environment.prod.ts`.

- [ ] **9. Modelos TypeScript y Servicios API**
  - [ ] Definir interfaces en `src/app/models/`: `Alumno`, `Pace`, `AlumnoPace`, `Meta`, `Merito`, `Staff`, `DashboardResumen`, `Alerta`.
  - [ ] Crear servicios en `src/app/core/services/`: `AlumnosService`, `PacesService`, `ProgresoService`, `MeritosService`, `DashboardService`, `AuthService`.
  - [ ] Cada servicio usa `HttpClient` e inyecta la `baseUrl` desde el environment.
  - [ ] **Interceptor JWT** (`JwtInterceptor`): adjunta `Authorization: Bearer <token>` en cada request saliente. Registrar en `app.config.ts`.
  - [ ] **Interceptor de errores** (`ErrorInterceptor`): captura errores HTTP globalmente:
    - **401 Unauthorized** → limpiar token de `localStorage` + navegar a `/login`. Cubre dos escenarios: credenciales incorrectas en login *y* token expirado mientras la monitora está en sesión activa en el salón.
    - **403 Forbidden** → toast de advertencia "No tienes permiso para realizar esta acción" sin redirigir.
    - **5xx Server Error** → toast de error genérico con opción de reintentar la última acción.

- [ ] **10. Componentes Base (Design System — `SharedModule`)**
  - [ ] `ButtonComponent` — variantes de Input: `variant` (primary, secondary, danger, ghost), `loading`, `disabled`.
  - [ ] `BadgeComponent` — variante semántica por Input: green, orange, red, blue, gray.
  - [ ] `CardComponent` — contenedor base con shadow y border-radius como ng-content wrapper.
  - [ ] `ProgressBarComponent` — Input `value` (0–100); clase CSS de color calculada automáticamente por umbral.
  - [ ] `ToastService` + `ToastComponent` — servicio inyectable para disparar notificaciones; máximo 3 simultáneas con auto-dismiss.
  - [ ] `ModalComponent` — wrapper con `@Output() confirmed` y `@Output() cancelled`; usado para confirmaciones destructivas.
  - [ ] `AvatarComponent` — genera iniciales y color de fondo determinístico a partir del nombre del alumno.
  - [ ] `SpinnerComponent` — variantes inline y overlay de pantalla completa.

- [ ] **11. Autenticación y Guards**
  - [ ] `AuthService`: métodos `login()`, `logout()`, `getToken()`, `getRole()`, `isAuthenticated()`. Token guardado en `localStorage`.
  - [ ] `AuthGuard` (`CanActivateFn`): redirige a `/login` si no hay token válido.
  - [ ] `RoleGuard` (`CanActivateFn`): recibe `data.roles` desde la config de rutas; redirige a `/dashboard` si el rol no tiene acceso.
  - [ ] Directiva `*hasRole` (o pipe): oculta elementos del template según el rol del usuario autenticado.

- [ ] **12. Módulo de Login**
  - [ ] `LoginComponent` con `ReactiveForm`: email, contraseña, selector de rol (visual).
  - [ ] Validación en tiempo real: email con formato válido, contraseña mínimo 6 caracteres.
  - [ ] Llamada a `POST /api/v1/Auth/login`; en éxito guardar token y navegar según rol.
  - [ ] Manejo de error 401: mensaje inline "Credenciales incorrectas" sin recargar la página.
  - [ ] Overlay de primera vez (heurística Nielsen #10) al primer login del usuario.

- [ ] **13. Módulo Dashboard**
  - [ ] `DashboardComponent`: orquesta llamadas paralelas con `forkJoin` a `DashboardService.getResumen()` y `DashboardService.getAlertas()`.
  - [ ] `MetricCardComponent`: tarjeta reutilizable para las 4 métricas (alumnos activos, metas hoy, PACEs en revisión, alertas).
  - [ ] `AlertBannerComponent`: muestra alumnos sin meta 2+ días; se oculta si no hay alertas.
  - [ ] `AlumnosTableComponent`: tabla con paginación, filtro por nivel, navegación al perfil con `routerLink`.
  - [ ] `WeeklyChartComponent`: gráfica de barras semanales en CSS puro (sin librería de charts).
  - [ ] `ScorePendingListComponent`: panel de PACEs pendientes de Score Station.
  - [ ] Ruta protegida con `RoleGuard` para roles Principal y Supervisora.

- [ ] **14. Módulo Alumno (Perfil)**
  - [ ] `AlumnoProfileComponent`: obtiene el alumno por `id` desde `ActivatedRoute.params`, carga datos con `AlumnosService`.
  - [ ] `AlumnoHeroComponent`: avatar, nombre, grado, fecha de ingreso, chips de privilegios activos/inactivos.
  - [ ] `PaceCardComponent`: tarjeta de PACE con `ProgressBarComponent`, estado y botones de acción (Score Station, registrar puntos). Emite `@Output` para las acciones.
  - [ ] `GoalChecklistComponent`: lista de metas del día separadas por turno; toggle con llamada a `PUT /api/v1/Progreso/metas/{metaId}/estatus`.
  - [ ] `AddGoalFormComponent`: `ReactiveForm` inline; llama `POST /api/v1/Progreso`.
  - [ ] `QuickActionsComponent`: botones para otorgar mérito, registrar demérito, actualizar puntos — abren `ModalComponent` para confirmar.
  - [ ] `MeritLogComponent`: lista de los últimos 10 méritos/deméritos del alumno.
  - [ ] Elementos destructivos condicionados a rol con la directiva `*hasRole`.

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
