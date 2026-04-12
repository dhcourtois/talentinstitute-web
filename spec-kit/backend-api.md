# Especificación de la API Backend - Talent Institute

Este documento define la arquitectura y los contratos principales del backend, respetando los principios establecidos en `constitution.md`. Todo el desarrollo se realizará siguiendo **Test-Driven Development (TDD)** de forma obligatoria.

## 🏗 Estructura: Arquitectura Limpia (Clean Architecture)

El proyecto backend (.NET 8) se dividirá en cuatro capas principales:

1. **Domain (Core):**
   - El corazón de negocio; no depende de ninguna otra capa.
   - Contiene las interfaces y las entidades de negocio esenciales del sistema ACE:
     - `Alumno`: Representa al estudiante.
     - `PACE`: Material didáctico/módulo de estudio individual asíncrono.
     - `Meta`: Objetivos diarios/semanales establecidos y cumplidos por el alumno.
     - `Merito`: Registro de comportamiento, logros académicos o sanciones disciplinarias.

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
1. **Principal:** Acceso administrativo total. Funciones de revisión de reportes globales estadísticos y control del personal.
2. **Supervisora:** Autoridad académica del aula principal. Encargada de la asignación de PACES, validación y aprobación de metas estratégicas y evaluación del desempeño.
3. **Monitora:** Personal de apoyo en el aula. Funciones de revisión de avances diarios, calificación operativa (scoring), registro de asistencia y asignación directa de méritos o deméritos rápidos.

## 📡 Endpoints Principales (REST)

Se utilizará como base el ruteo `/api/v1/`.

### 1. Autenticación (`/api/v1/Auth`)
- `POST /login`: Autentica al staff y devuelve el token JWT conteniendo los *claims* identificativos y el rol asignado al usuario.

### 2. Registro de Avances Semanales (`/api/v1/Progreso`)
- `POST /`: Registra o aprueba el avance programado para la semana en el panel de metas de un alumno.
- `GET /{alumnoId}/semana/{fechaInicio}`: Recupera el estatus de las metas de un alumno enfocadas a una semana temporal.
- `PUT /metas/{metaId}/estatus`: Permite a la Monitora/Supervisora marcar o verificar el estado final (Scored, Corrected, Tested) de la meta de un PACE.

### 3. Gestión de Méritos y Deméritos (`/api/v1/Meritos`)
- `POST /`: Registra un nuevo evento de mérito o demérito en el expediente de un alumno. (Acceso: Supervisora, Monitora).
- `GET /alumno/{alumnoId}`: Obtiene el balance actual y el historial cronológico de comportamiento.
- `PATCH /{meritoId}/revocar`: Revoca un registro mal ingresado o condonado (Acceso estricto: Principal o Supervisora).

## 🧪 Especificación de Pruebas TDD

El Testing no es algo implementado a posteriori; es quien rige el ciclo de software en 'Talent Institute'.

- **Herramientas Principales:** `xUnit` como orquestador de pruebas y afirmaciones, y `Moq` para el aislamiento de dependencias.
- **Métricas:** Es obligatorio apuntar y mantener un **>90% de cobertura (Code Coverage)** en la vital capa de `Application` (Casos de uso). El código no llegará a la rama principal (main) si rompe esta regla.
- **Estrategia General (Red - Green - Refactor):**
  - **Dominio:** Orientado a pruebas de comportamiento que verifiquen el estado interno puro de la entidad (ej: no se pueden asignar méritos con cantidad cero o negativa). Nada de mocking necesario aquí.
  - **Application (Use Cases):** Validar orquestación. Usar `Moq` para inyectar depósitos de datos efímeros y evaluar flujos (ej: si un avance se registra válido, se llama al `IProgresoRepository.SaveAsync()`).
