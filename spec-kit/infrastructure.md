# Especificación de Infraestructura y Despliegue - Talent Institute

Este documento detalla la topología de la nube, la gestión de la base de datos y la automatización del ciclo de vida de la aplicación. Mantendremos la filosofía central de que todo debe ser codificado, testeado y versionado.

## 🗄 Base de Datos: Azure SQL

La persistencia será gestionada en **Azure SQL Database**. Se utilizará el enfoque *Code-First* mediante *Entity Framework Core*, garantizando integridad referencial y convenciones consistentes.

### Diseño de la Base de Datos (Esquema ACE)

Se utilizará el estándar **PascalCase** para todas las tablas y columnas, unificando la convención entre C# y la base de datos. 

#### 1. Entidades del Sistema
- **Users:** Gestión de Identity y Staff del colegio.
  - Columnas: `Id`, `Email`, `PasswordHash`, `Role` (Principal, Supervisora, Monitora), `IsActive`.
- **Students:** Datos generales de los alumnos inscritos.
  - Columnas: `Id`, `EnrollmentNumber`, `FirstName`, `LastName`, `CurrentLevel`, `PrivilegeStatus` (Estatus dentro del sistema de privilegios).
- **Paces:** Catálogo central de módulos de estudio estandarizados.
  - Columnas: `Id`, `Subject` (Materia), `PaceNumber` (ej. 1045).

#### 2. Entidades Relacionales y Transaccionales
- **StudentProgress:** Tabla relacional vital. Traza el ciclo de vida de un PACE asignado a un estudiante en particular.
  - Columnas: `Id`, `StudentId` (FK), `PaceId` (FK), `StartDate`, `CompletionDate`, `FinalScore`, `Status` (Assigned, InProgress, SelfTest, FinalTestPassed).
- **DailyGoals:** Registro granular táctico de las metas escolares programadas.
  - Columnas: `Id`, `StudentProgressId` (FK), `TargetPages` (Cantidad o página), `TargetDate`, `GoalStatus` (Pending, Completed, Scored).
- **MeritsDemerits:** Bitácora para el control de incidencias, alimentando el `PrivilegeStatus` del estudiante.
  - Columnas: `Id`, `StudentId` (FK), `UserId` (FK a responsable Staff), `RecordType` (Merit / Demerit), `Points`, `AppliedDate`, `Observations`.

### Diccionario de Datos SQL crítico y Mejores Prácticas

- `UNIQUEIDENTIFIER`: Utilizado como Llave Primaria (PK) en entidades transaccionales (`Students.Id`, `StudentProgress.Id`, `DailyGoals.Id`). Evita colisiones y simplifica la creación de entidades desconectadas en C# para los Mocks/Tests.
- `INT / INT IDENTITY`: Para catálogos o enumeradores puros (`Paces.Id`, `Points`).
- `NVARCHAR(n)`: Para strings de longitud acotada (nombres, subject). Usaremos `NVARCHAR(MAX)` restrictivamente solo para `Observations`.
- `DATETIME2`: Preferido antes que datetime clásico por su mayor precisión y alineamiento con el `DateTime` de .NET 8, usado en `StartDate` y `AppliedDate`.
- `DATE`: Para `TargetDate`, la meta se evalúa por el día entero, por lo que la porción de tiempo estorba.
- `DECIMAL(5,2)`: Para la captura métrica de `FinalScore` permitiendo rangos de 0.00 a 100.00.

*(Nota de Integridad): Todas las relaciones inter-tabla configuradas por EF Core estarán blindadas en Azure SQL mediante reglas preventivas `ON DELETE RESTRICT`. Esto asegura que los Unit Tests de los Repositorios y el comportamiento en producción en la capa transaccional sean 100% predecibles.*

## 🔐 Gestión de Secretos y Credenciales

Para garantizar una postura de seguridad incuestionable, acorde a los protocolos Enterprise de .NET, el manejo de credenciales se rige bajo las siguientes políticas:

- **Localización Centralizada:** Todo archivo que posea información sensible (ej. el Publish Profile `.PublishSettings`, Service Principals, o las cadenas de conexión a Azure SQL) debe residir **exclusivamente en la carpeta `/secrets`** en la raíz del espacio de trabajo. Las herramientas (e inteligencias artificiales) deben buscar allí para leer configuraciones a aplicar en despliegues.
- **Seguridad y Aislamiento (Git):** La ruta `/secrets` está **estrictamente excluida** del control de versiones (`.gitignore`). 
  > *Restricción del Sistema (IA y Humana): Queda terminantemente prohibido generar código o salidas que reproduzcan, expongan o copien los valores legibles de esta carpeta dentro de la documentación general o el repositorio. La documentación solo debe hacer referencia a la ubicación.*
- **Mapeo en la Aplicación (.NET 8):**
  - **Local:** Está vetado guardar estas llaves en el `appsettings.json`. El setup del proyecto usará el comando `dotnet user-secrets` para trazar los valores de `/secrets` hacia los secretos de usuario del sistema operativo local.
  - **Producción:** Los valores oficiales de Azure y las llaves maestras se provisionarán cargándose dentro de **Azure Key Vault** para el entorno definitivo que corre bajo la Web App en Linux.

## ☁️ Hosting y Cómputo: Azure Web App

- El sistema (Backend .NET 8 y la SPA pre-empaquetada) será ensamblado y hospedado de manera administrada (PaaS) en **Azure Web App (Linux Web Plan)**.
- **Auto-Scaling (Escalado Automático):** Vital para nuestro caso de uso. El salón ACE y las áreas administrativas generan picos masivos de uso durante minutos muy específicos (ej: el "check-in" de la mañana y la firma final de progreso antes de la salida). El servicio estará configurado para escalar horizontalmente sus instancias frente a picos repentinos de CPU o RAM.

## 🏗 Infraestructura como Código (IaC)

- **Cero Intervención Manual:** Queda estrictamente prohibido aprovisionar recursos mediante clics en el Azure Portal (ClickOps).
- **Herramienta:** Todo recurso (SQL Server, Databases, App Services, Application Insights) será definido utilizando **Bicep** (el lenguaje de plantillas nativo de Microsoft).
- Estos scripts vivirán en la subcarpeta `/infra` dentro del repositorio, evolucionando a la par del código.

## 🚀 Integración y Despliegue Continuo (CI/CD)

Utilizaremos **GitHub Actions** como nuestro motor de pipelines para validar y liberar versiones de forma determinista.

### Pipeline Estándar (Al fusionar a Main)
1. **Build & Compile:** Restauración de paquetes (Nuget/NPM) y compilación limpia.
2. **Quality Gate (La Barrera TDD):** 
   - Se ejecutan los test automatizados completos de la capa de Dominio y Aplicación.
   - **Regla Estricta:** Si una sola prueba (Unit Test) falla, *la pipeline se aborta e impide el despliegue a producción de manera irrevocable.*
3. **Infra Update:** Se validan e inyectan los templates Bicep para asegurar que la estructura en Azure sea exactamente la especificada en los archivos actuales.
4. **Entity Framework Migrations:** Se aplican cambios diferenciales seguros al esquema en la base de datos viva.
5. **Release:** Sustitución sin tiempo de inactividad (Zero-downtime) de los binarios en la granja del Azure Web App.
