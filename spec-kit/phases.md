# Fases de Desarrollo - Talent Institute

Este documento describe la hoja de ruta estratégica para la construcción del sistema ACE.

> **Referencia obligatoria:** Antes de implementar cualquier entidad, estado o regla de negocio, consultar `domain-rules.md`. Es la fuente de verdad para naming canónico, máquinas de estado, invariantes y reglas ejecutables.

## Fase 1: Cimiento (Infraestructura y Auth)
*   **Aprovisionamiento Inicial:** Creación del repositorio, pre-configuración inicial de la solución .NET 8 con las cuatro capas de la Arquitectura Limpia.
*   **Infrastructure as Code:** Creación de los scripts Bicep y el Pipeline de GitHub Actions base.
*   **Identidad Global:** Desarrollo de las entidades de Staff y roles iniciales (Supervisora, Monitora, Principal).
*   **Seguridad y Autenticación:** Implementación de generación y validación de tokens JWT. Configuración de Políticas basadas en Claims en el API.

## Fase 2: Core (Lógica de PACEs y Metas)
*   **Modelado del Sistema ACE:** Creación de las entidades nativas puras: `Alumno`, `Pace`, vínculo `AlumnoPace`, `Meta` diaria/semanal.
*   **Desarrollo Dirigido por Pruebas (TDD):** Implementación de Unit Tests obligatorios para validar férreamente las reglas de negocio sobre balances y avances.
*   **Casos de Uso Principales:** Desarrollo en la capa de Aplicación de los flujos de "Registro de Meta" y "Aumento de Scoring".
*   **Persistencia (EF Core):** Configuración de Entity Framework Core, Model Builder, Migraciones y adaptadores hacia Azure SQL Database.

## Fase 3: Operación (Méritos y Frontend Angular)
*   **Ecosistema Disciplinario:** Entidades y persistencia para la asignación y seguimiento de la bolsa de Méritos/Deméritos.
*   **Frontend SPA con Angular 18:** Proyecto Angular inicializado con routing, modo estricto y CSS por componente. Incluye: interceptores JWT, guards de rol (`AuthGuard`, `RoleGuard`), shared design system y los dos módulos principales:
    - `DashboardModule`: métricas, tabla de alumnos, alertas y gráfica semanal.
    - `AlumnoModule`: perfil completo, PACEs, checklist de metas, méritos y acciones rápidas.
*   **Integración Dinámica:** Todos los servicios Angular conectados a los endpoints del backend REST mediante `HttpClient`. Build de producción (`ng build --configuration production`) integrado en el pipeline CI/CD.

## Fase 4: Cierre (Pruebas E2E y Documentación)
*   **Testing Funcional Integral:** Ejecución de pruebas End-to-End (E2E) simulando el flujo caótico de una oficina en hora pico ("Lunes en la mañana" y "Viernes al cierre").
*   **Refactorización UX:** Revisión final del complimiento de las 10 Heurísticas de Nielsen (Validación de barreras ante errores humanos y recordatorios inline).
*   **Despliegue a Producción:** Aplicación automática por GitHub Actions hacia Azure Web Apps mediante el pipeline seguro, tras pasar los controles de Cobertura Unit Test.
