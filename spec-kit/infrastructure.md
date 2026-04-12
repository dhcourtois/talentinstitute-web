# Especificación de Infraestructura y Despliegue - Talent Institute

Este documento detalla la topología de la nube, la gestión de la base de datos y la automatización del ciclo de vida de la aplicación. Mantendremos la filosofía central de que todo debe ser codificado, testeado y versionado.

## 🗄 Base de Datos: Azure SQL

La persistencia será gestionada en **Azure SQL Database**. Se utilizará el enfoque *Code-First* mediante *Entity Framework Core*, garantizando integridad referencial y convenciones consistentes.

### Diseño de la Base de Datos (Esquema ACE)

Se utilizará el estándar **PascalCase** para todas las tablas y columnas, unificando la convención entre C# y la base de datos. 

> Los nombres de tablas y columnas siguen el glosario canónico de `domain-rules.md`. El mapeo EF Core es directo (Spanish → Spanish), sin capa de traducción.

#### 1. Entidades del Sistema
- **Staff:** Gestión de usuarios del colegio con acceso al sistema.
  - Columnas: `Id`, `Email`, `PasswordHash`, `Rol` (`Principal`, `Supervisora`, `Monitora`), `Activo`, `RowVersion`.
- **Alumnos:** Datos generales de los alumnos inscritos.
  - Columnas: `Id`, `NumeroMatricula`, `Nombre`, `Apellido`, `Nivel`, `PrivilegioStatus`, `RowVersion`.
  - `PrivilegioStatus`: campo de bits (`INT`). Calculado exclusivamente por `Alumno.RecalcularPrivilegios(balance, config)` en el Domain. Los umbrales viven en `ConfiguracionPrivilegios`, no hardcodeados aquí. Ver `domain-rules.md` §4.
- **Paces:** Catálogo central de módulos de estudio estandarizados.
  - Columnas: `Id`, `Materia`, `NumeroPace` (ej. 1045), `PuntajeMaximo`.
- **ConfiguracionPrivilegios:** Umbrales configurables para el cálculo de privilegios.
  - Columnas: `Id`, `UmbralOficina`, `UmbralOficinaRevocado`, `UmbralComedor`, `UmbralComedorRevocado`, `UmbralPatio`, `UmbralPatioRevocado`, `UmbralBiblioteca`, `UmbralBibliotecaRevocado`, `UmbralActividades`, `UmbralActividadesRevocado`, `FechaActualizacion`, `StaffIdActualizo` (FK).
  - Solo existe un registro activo a la vez. El Principal es el único autorizado a modificarlo.

#### 2. Entidades Relacionales y Transaccionales
- **AlumnoPaces:** Traza el ciclo de vida de un PACE asignado a un alumno específico.
  - Columnas: `Id`, `AlumnoId` (FK), `PaceId` (FK), `FechaInicio`, `FechaCompletado`, `PuntajeFinal`, `Estado` (`Asignado`, `EnProgreso`, `ListoParaAutoTest`, `AutoTestOk`, `AutoTestFallido`, `EnTestFinal`, `Completado`, `Fallido`), `RowVersion`.
- **Metas:** Registro diario de los compromisos de trabajo del alumno.
  - Columnas: `Id`, `AlumnoPaceId` (FK), `Turno` (`Mañana`, `Tarde`), `PaginasObjetivo`, `FechaObjetivo` (`DATE`), `PuntajeObtenido`, `Estado` (`Pendiente`, `EnProgreso`, `Completada`, `Rechazada`, `Scored`, `Aprobada`).
- **Meritos:** Bitácora de méritos y deméritos; fuente del balance que alimenta `PrivilegioStatus`.
  - Columnas: `Id`, `AlumnoId` (FK), `StaffId` (FK al responsable), `Tipo` (`Merito`, `Demerito`), `Puntos`, `Motivo`, `FechaAplicado` (`DATETIME2`), `Revocado` (`BIT`), `StaffIdRevoco` (FK, nullable), `FechaRevocacion` (nullable).

### Diccionario de Datos SQL crítico y Mejores Prácticas

- `UNIQUEIDENTIFIER`: Llave Primaria (PK) en entidades transaccionales (`Alumnos.Id`, `AlumnoPaces.Id`, `Metas.Id`, `Meritos.Id`). Evita colisiones y simplifica la creación de entidades desconectadas en C# para los Mocks/Tests.
- `INT / INT IDENTITY`: Para catálogos o enumeradores puros (`Paces.Id`, `Puntos`).
- `NVARCHAR(n)`: Para strings de longitud acotada (`Nombre`, `Apellido`, `Materia`). Usar `NVARCHAR(MAX)` restrictivamente solo para `Motivo` en `Meritos`.
- `DATETIME2`: Preferido sobre `datetime` clásico por mayor precisión y alineamiento con `DateTime` de .NET 8. Usado en `FechaInicio`, `FechaAplicado`, `FechaRevocacion`.
- `DATE`: Para `FechaObjetivo` en `Metas` — la meta se evalúa por el día entero; la porción de tiempo estorba.
- `DECIMAL(5,2)`: Para `PuntajeFinal` en `AlumnoPaces` y `PuntajeObtenido` en `Metas`, permitiendo rangos de 0.00 a 100.00.
- `ROWVERSION` / `TIMESTAMP`: En `Alumnos`, `AlumnoPaces` y `Meritos` para concurrencia optimista (ver `domain-rules.md` §7).

*(Nota de Integridad): Todas las relaciones inter-tabla configuradas por EF Core estarán blindadas en Azure SQL mediante reglas preventivas `ON DELETE RESTRICT`. Esto asegura que los Unit Tests de los Repositorios y el comportamiento en producción en la capa transaccional sean 100% predecibles.*

## 🔐 Gestión de Secretos y Credenciales

Para garantizar una postura de seguridad incuestionable, acorde a los protocolos Enterprise de .NET, el manejo de credenciales se rige bajo las siguientes políticas:

- **Localización Centralizada:** Todo archivo que posea información sensible (ej. el Publish Profile `.PublishSettings`, Service Principals, o las cadenas de conexión a Azure SQL) debe residir **exclusivamente en la carpeta `/secrets`** en la raíz del espacio de trabajo. Las herramientas (e inteligencias artificiales) deben buscar allí para leer configuraciones a aplicar en despliegues.
- **Seguridad y Aislamiento (Git):** La ruta `/secrets` está **estrictamente excluida** del control de versiones (`.gitignore`). 
  > *Restricción del Sistema (IA y Humana): Queda terminantemente prohibido generar código o salidas que reproduzcan, expongan o copien los valores legibles de esta carpeta dentro de la documentación general o el repositorio. La documentación solo debe hacer referencia a la ubicación.*
- **Mapeo en la Aplicación (.NET 8):**
  - **Local:** Está vetado guardar estas llaves en el `appsettings.json`. El setup del proyecto usará el comando `dotnet user-secrets` para trazar los valores de `/secrets` hacia los secretos de usuario del sistema operativo local.
  - **Producción:** Los valores oficiales de Azure y las llaves maestras se provisionarán cargándose dentro de **Azure Key Vault** para el entorno definitivo que corre bajo la Web App en Linux.

## 🗂 Tiers de Azure Recomendados

| Recurso | Tier recomendado | Justificación |
|---|---|---|
| **Azure SQL Database** | **Basic (5 DTUs)** en desarrollo; **Standard S1 (20 DTUs)** en producción | El volumen de <150 alumnos genera carga transaccional baja. S1 da margen de crecimiento y permite backups automáticos con retención de 35 días. |
| **Azure Web App** | **Free (F1)** en staging; **Basic B1** en producción | B1 es el tier mínimo que soporta slots de deployment para zero-downtime y tiene suficiente RAM (1.75 GB) para el runtime de .NET 8 + la SPA. |
| **Azure Key Vault** | **Standard** | Tier más bajo disponible; suficiente para el volumen de secretos de este sistema. |
| **Application Insights** | Workspace-based (pay-as-you-go) | El volumen de telemetría será mínimo; el costo efectivo será <$5 USD/mes. |

> **Costo total estimado en producción:** $30–80 USD/mes dependiendo del uso de DTUs en SQL y del tráfico de Application Insights.

## 💾 Política de Respaldo (Backup)

- **Azure SQL — Backups automáticos:** El tier Standard S1 incluye backups automáticos gestionados por Azure con retención de **35 días** (point-in-time restore). No requiere configuración adicional.
- **Retención extendida:** No es necesaria en esta fase; el historial de 35 días es suficiente para un colegio con ciclo escolar definido.
- **Backup manual previo a migraciones:** Antes de cada ejecución de migraciones EF Core en producción, el pipeline de CI/CD debe tomar un snapshot manual de la base de datos como paso previo (implementado como tarea en el workflow de GitHub Actions).
- **Repositorio como backup de código:** Todo el código, scripts Bicep y migraciones viven en GitHub. La infraestructura completa es reproducible desde cero ejecutando los scripts Bicep y el comando `dotnet ef database update`.

## ☁️ Hosting y Cómputo: Azure Web App

- El sistema se despliega como una **sola unidad** en **Azure Web App (Linux Web Plan)**:
  - El build de Angular (`ng build --configuration production`) genera archivos estáticos en `dist/`. Estos se copian a `wwwroot/` del proyecto .NET API antes del despliegue.
  - El middleware de ASP.NET Core sirve los archivos estáticos de Angular y configura el fallback a `index.html` para que el router del lado del cliente funcione correctamente (`app.UseStaticFiles()` + `app.MapFallbackToFile("index.html")`).
  - Resultado: un solo Azure Web App sirve tanto la API REST como la SPA, simplificando la infraestructura y eliminando la necesidad de configurar CORS entre dominios distintos en producción.
- **Auto-Scaling (Escalado Automático):** El salón ACE genera picos de uso en momentos específicos (check-in de la mañana, cierre de progreso al final del día). El Web App estará configurado para escalar horizontalmente frente a picos repentinos de CPU o RAM.

## 🏗 Infraestructura como Código (IaC)

- **Cero Intervención Manual:** Queda estrictamente prohibido aprovisionar recursos mediante clics en el Azure Portal (ClickOps).
- **Herramienta:** Todo recurso (SQL Server, Databases, App Services, Application Insights) será definido utilizando **Bicep** (el lenguaje de plantillas nativo de Microsoft).
- Estos scripts vivirán en la subcarpeta `/infra` dentro del repositorio, evolucionando a la par del código.

## 🚀 Integración y Despliegue Continuo (CI/CD)

Utilizaremos **GitHub Actions** como nuestro motor de pipelines para validar y liberar versiones de forma determinista.

### Pipeline Estándar (Al fusionar a Main)
1. **Build Frontend (Angular):** `npm ci` + `ng build --configuration production`. Los artefactos de `dist/` se copian a `wwwroot/` del proyecto .NET API.
2. **Build & Compile Backend (.NET):** Restauración de paquetes NuGet y compilación limpia incluyendo los archivos estáticos de Angular ya copiados.
3. **Quality Gate (La Barrera TDD):**
   - Se ejecutan los tests automatizados completos de la capa de Dominio y Aplicación (xUnit).
   - **Regla estricta:** Si una sola prueba falla, el pipeline se aborta e impide el despliegue de manera irrevocable.
4. **Infra Update:** Validación e inyección de los templates Bicep para asegurar que la infraestructura en Azure coincide exactamente con los archivos actuales.
5. **Entity Framework Migrations:** Snapshot manual de la base de datos de producción → aplicación de migraciones diferenciales.
6. **Release:** Sustitución zero-downtime de los binarios (backend + SPA compilada) en el Azure Web App.
