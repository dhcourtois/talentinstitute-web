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
  - [ ] Test Red: Escribir pruebas para el comportamiento del ciclo vital del material `PACE`.
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

- [ ] **7. Seguridad e Implementación**
  - [ ] Verificar la existencia de la carpeta `/secrets` localmente.
  - [ ] Configurar el perfil de publicación de Azure desde el archivo en `/secrets`.
  - [ ] Configurar las variables de entorno en el Azure Web App utilizando los valores del Service Principal.
