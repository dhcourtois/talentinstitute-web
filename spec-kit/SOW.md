# Statement of Work (SOW)
## Sistema de Gestión Institucional — Talent Institute

---

| | |
|---|---|
| **Documento** | Statement of Work v1.0 |
| **Proyecto** | Sistema Institucional Talent Institute |
| **Cliente** | Talent Institute — Instituto Cristiano en Los Héroes |
| **Dirección** | Calle 83 No. 492, Fracc. Los Héroes, C.P. 97306, Mérida, Yucatán |
| **Contacto cliente** | 55 7505 9352 |
| **Fecha de emisión** | Abril 2026 |
| **Versión** | 1.0 |

---

## 1. Antecedentes y Contexto

Talent Institute es un colegio cristiano ubicado en Mérida, Yucatán, que opera bajo el sistema educativo **ACE (Accelerated Christian Education)**. A diferencia del modelo tradicional de clases grupales, el sistema ACE se basa en:

- **PACEs**: módulos de autoaprendizaje individualizados por materia y nivel.
- **Metas diarias/semanales**: cada alumno fija y cumple objetivos concretos de avance.
- **Score Stations**: estaciones de autoevaluación donde el alumno califica su propio trabajo antes de que la supervisora valide el resultado.
- **Sistema de Méritos y Deméritos**: mecanismo de reconocimiento y corrección de comportamiento vinculado directamente a los privilegios del alumno (acceso a espacios como oficina, comedor, patio, etc.).
- **Progresión individualizada**: cada alumno avanza a su propio ritmo, sin depender del grupo.
- **Tabla de PACE's**:
* Kindergarten/ Reading Readiness            1-12  (Colorear, tomamos ejemplos de paces de ABC, libros de apoyo) 
* Kinder 3/ABC                               1-12  (Construccion de palabras, Matematicas, Naturales, Sociales, Speaking English)
* 1 Primaria/ ABC 6 a 7 años                 1-12  (Español, Animal Science, Bible REading, Math, Science, Social)
* 2 Primaria/ Elementary 1  (7 a 8 años)     13-24 (Math, English, WB, Science, Social, Español)
* 3 Primaria/ Elementary 1  (8 a 9 años)     25-36 (Math, English, WB, Science, Social, Español)
* 4 Primaria/ Elementary 2  (9 a 10 años)    37-48 (Math, English, WB, Science, Social, Español)
* 5 Primaria/ Elementary 2  (10 a 11 años)   49-60 (Math, English, WB, Science, Social, Español)
* 6 Primaria/ Elementary 3  (11 a 12 años)   61-72 (Math, English, WB, Science, Social, Español)
* 1 Secundaria/                              73-85 (Math, English, WB, Science, Social, Español)
* 2 Secundaria/                              85-94
* 3 Secundaria/                              94-108
* 1 Preparatoria/                            108-120
* 2 Preparatoria/                            120-132
* 3 Preparatoria/                            133-144
  
Actualmente el seguimiento de este proceso se realiza de forma manual, lo que genera fricciones operativas, falta de visibilidad del progreso en tiempo real, y dificultad para detectar alumnos en riesgo de estancamiento. El presente proyecto tiene como objetivo **digitalizar y centralizar** la gestión de este modelo pedagógico.

---

## 2. Objetivo del Proyecto

Desarrollar una **aplicación web institucional** (Sistema Institucional) que permita al personal de Talent Institute:

1. Registrar y monitorear el avance individual de cada alumno en sus PACEs activos.
2. Gestionar las metas diarias y el puntaje acumulado por semana.
3. Administrar el sistema de méritos y deméritos con trazabilidad completa.
4. Visualizar alertas tempranas sobre alumnos estancados o en riesgo.
5. Controlar privilegios de los alumnos con base en su comportamiento y cumplimiento de metas.

El sistema deberá ser operado desde tablets (iPad, prioritario) y computadoras de escritorio por tres perfiles de usuario con distintos niveles de acceso.

---

## 3. Alcance del Trabajo

### 3.1 Incluido en el proyecto

#### Módulo 1 — Autenticación y Control de Acceso
- Inicio de sesión con correo y contraseña.
- Generación y validación de tokens JWT (stateless).
- Tres roles iniciales configurados (sin módulo de creación dinámica por ahora, se dejará la opción para más adelante):
  - **Principal** — sin restricciones.
  - **Supervisora** — vistas a ciertas partes (registro de calificaciones, avances semanales, estatus, etc.).
  - **Monitora** — acceso al perfil del alumno (deméritos, méritos, etc.).
- Cierre de sesión y expiración automática de sesión.
- Portal o acceso para padres de familia / alumnos.

#### Módulo 1.5 — Entrevistas a Padres de Familia
- Registro de información de familias interesadas/entrevistas iniciales.
- Campos clave: número de hijos, y espacio para comentarios críticos.
- Registro de factores de riesgo en el hogar: violencia familiar, divorcios, conocimiento de Dios (vital para la seguridad del personal ante posibles episodios de agresividad por parte de los padres).
- Bandera de aceptado/rechazado e índices de alerta.
- Registro de comentarios para anlisis de estudio socioeconomico

#### Módulo 2 — Gestión de Alumnos
- Alta, edición y consulta de perfil del alumno (datos generales, grado, nivel).
- Vista de perfil individual con historial completo.
- Indicador de privilegios activos e inactivos por alumno.
- Lista de alumnos lista para exportar e imprimir

#### Módulo 3 — PACEs y Progreso Académico
- Asignación de PACEs por alumno y materia.
- Registro de puntos por avance semanal (escala 0–50 por PACE).
- Control de estado del PACE: En progreso / Listo para Score Station / Completado /Calificado. (se debera de contemplar un historial de visualizacion paso a paso al padre de familia) 
- Historial de PACEs completados por alumno.

#### Módulo 4 — Metas Diarias
- Registro y validación de metas por turno (mañana/tarde).
- Indicador de cumplimiento diario y semanal.
- Alertas automáticas cuando un alumno lleva 2 o más días sin registrar meta.
- Mensajería interna o notificaciones push.

#### Módulo 5 — Méritos y Deméritos
- Registro de mérito o demérito con: motivo, cantidad de puntos, responsable, fecha/hora.
- Balance neto visible en el perfil del alumno.
- Actualización automática de privilegios según balance y reglas configuradas.
- Historial completo auditable.

#### Módulo 6 — Dashboard General (Supervisora / Principal)
- Resumen ejecutivo: alumnos activos, metas cumplidas hoy, PACEs en revisión, alertas.
- Tabla de progreso semanal de todos los alumnos con filtros y búsqueda.
- Lista de alertas activas ordenadas por urgencia.
- Indicador de PACEs pendientes de scoring.
- Gráfica semanal de metas cumplidas.

#### Módulo 7 — Infraestructura y Despliegue
- Arquitectura en Azure: Azure SQL Database + Azure Web App (Linux).
- Pipeline CI/CD con GitHub Actions: build, pruebas, migraciones, despliegue.
- Gestión de secretos con Azure Key Vault.
- Infraestructura como código con Bicep (cero clics manuales en el portal de Azure).
- Configuración de Application Insights para monitoreo básico.

### 3.2 Fuera del alcance

Los siguientes elementos **no están incluidos** en esta versión del proyecto y pueden considerarse para fases futuras:

- Aplicación móvil nativa (iOS / Android).
- Integración con plataformas externas de ACE (ACE Connect, PACEs Online, etc.).
- Generación de boletas, certificados o documentos escolares oficiales.
- Capacitación presencial in-situ (se incluye documentación de usuario y sesión de onboarding).

---

## 4. Entregables

| # | Entregable | Descripción | Fase |
|---|---|---|---|
| E-01 | Repositorio base | Estructura Clean Architecture (.NET 8) con pipeline CI/CD funcional | 1 |
| E-02 | Sistema de identidad | Login JWT con los 3 roles operando en ambiente de staging | 1 |
| E-03 | API Core | Endpoints de Alumnos, PACEs, Progreso y Metas con cobertura de pruebas >90% | 2 |
| E-04 | Migraciones de base de datos | Schema inicial en Azure SQL vía EF Core, reproducible desde cero | 2 |
| E-05 | API de Méritos/Deméritos | Endpoint completo con lógica de privilegios y auditoría | 3 |
| E-06 | Frontend SPA | Dashboard General + Vista de Perfil de Alumno, responsivo (tablet-first) | 3 |
| E-07 | Integración API–Frontend | Todas las vistas conectadas a los endpoints reales en staging | 3 |
| E-08 | Suite de pruebas E2E | Escenarios críticos automatizados cubriendo flujos de Supervisora y Monitora | 4 |
| E-09 | Despliegue en producción | Sistema funcionando en Azure Web App con dominio configurado | 4 |
| E-10 | Documentación de usuario | Guía de uso por rol (PDF/Notion), incluye flujo de onboarding | 4 |

---

## 5. Metodología de Desarrollo

El desarrollo se organiza en **4 fases secuenciales**, cada una con criterios de aceptación antes de avanzar a la siguiente.

### Fase 1 — Cimiento *(Foundation)*
**Objetivo:** Infraestructura, CI/CD e identidad.

- Creación del repositorio y estructura de solución .NET 8 con 4 capas (Domain, Application, Infrastructure, API).
- Configuración de proyectos de prueba (xUnit + Moq).
- Infraestructura en Azure via Bicep: SQL Server, Azure SQL Database, Web App, Application Insights.
- Pipeline de GitHub Actions: compilación → pruebas → validación Bicep → despliegue a staging.
- Entidad `Staff` con roles y autenticación JWT.
- Ambiente de staging funcional con login operativo.

**Criterio de aceptación:** El equipo del cliente puede iniciar sesión con los 3 roles en el ambiente de staging.

---

### Fase 2 — Core
**Objetivo:** Modelo de datos y lógica de negocio principal.

- Implementación TDD (Red–Green–Refactor) de entidades: `Alumno`, `Pace`, `AlumnoPace`, `Meta`.
- Repositorios e interfaces en Application Layer.
- Casos de uso: "Registro de Meta", "Avance de Scoring".
- Migraciones de base de datos con Entity Framework Core.
- Endpoints REST: `GET/POST/PUT /api/v1/Progreso`, `GET/POST /api/v1/Alumnos`.
- Documentación Swagger operativa.

**Criterio de aceptación:** Todos los endpoints del módulo Core responden correctamente con Postman; cobertura de pruebas >90% en Application Layer.

---

### Fase 3 — Operación
**Objetivo:** Méritos, frontend completo e integración.

- Implementación TDD de entidad `Merito` con lógica de privilegios.
- Endpoints: `GET/POST/PATCH /api/v1/Meritos`.
- Desarrollo del frontend SPA:
  - Dashboard General (Supervisora/Principal): métricas, tabla de alumnos, alertas, gráfica semanal.
  - Vista de Perfil de Alumno (Monitora): PACEs activos, checklist de metas, acciones rápidas, log de méritos.
- Conexión completa frontend–API en staging.
- Validación de usabilidad contra las 10 heurísticas de Nielsen.

**Criterio de aceptación:** El flujo completo (login → dashboard → perfil → registro de meta → mérito) opera sin errores en tablet iPad y desktop.

---

### Fase 4 — Cierre
**Objetivo:** Calidad, producción y transferencia.

- Suite de pruebas End-to-End con escenarios reales:
  - Supervisora asigna PACE y valida meta.
  - Monitora registra puntos de scoring y otorga mérito.
  - Principal consulta dashboard y gestiona usuarios de staff (crear, desactivar).
- Refinamientos finales de UX basados en revisión con el equipo del cliente.
- Despliegue en ambiente de producción con dominio definitivo.
- Configuración de Azure Key Vault para producción.
- Entrega de documentación de usuario por rol.
- Sesión de onboarding remoto (hasta 2 horas) con el personal del colegio.

**Criterio de aceptación:** El sistema opera en producción sin errores críticos durante una semana de uso real previo a la firma de aceptación formal.

---

## 6. Stack Tecnológico

| Capa | Tecnología |
|---|---|
| Backend | C# / ASP.NET Core 8 — Clean Architecture |
| Base de datos | Azure SQL Database (SQL Server) — Code-First via EF Core |
| Autenticación | JWT — Claims-based authorization |
| Frontend | Angular 18 + TypeScript — SPA tablet-first, sin framework de UI externo |
| Hosting | Azure Web App (Linux, PaaS) |
| Secretos | Azure Key Vault + `dotnet user-secrets` (local) |
| Infraestructura como código | Azure Bicep |
| CI/CD | GitHub Actions |
| Pruebas unitarias | xUnit + Moq |
| Pruebas E2E | Por definir con el equipo (Playwright recomendado) |
| Monitoreo | Azure Application Insights |

---

## 7. Cronograma Estimado

| Fase | Duración estimada | Hito de entrega |
|---|---|---|
| Fase 1 — Cimiento | 2–3 semanas | Login en staging operativo |
| Fase 2 — Core | 3–4 semanas | API Core completa con pruebas |
| Fase 3 — Operación | 4–5 semanas | Sistema integrado en staging |
| Fase 4 — Cierre | 2–3 semanas | Sistema en producción + onboarding |
| **Total** | **~13–15 semanas** | **~3.5 meses desde inicio** |

> Las fechas exactas se acordarán en el kickoff del proyecto y quedarán documentadas en un plan de proyecto compartido.

### Nota sobre complejidad y alcance

Este spec incluye decisiones de calidad que tienen un costo real en tiempo: TDD con >90% de cobertura, Clean Architecture completa, Angular con design system propio, Bicep, CI/CD con staging separado, Playwright E2E y revisión de 10 heurísticas de Nielsen. Todo esto es correcto para construir algo mantenible, pero es importante reconocer que para un colegio de <150 alumnos, parte de esta capa es inversión a futuro, no requisito inmediato.

**Si el tiempo es la restricción principal**, la siguiente capa "enterprise" puede diferirse a una Fase 5 sin afectar la funcionalidad central:
- Tests E2E con Playwright (reemplazable por pruebas manuales documentadas en Fase 4)
- Application Insights (reemplazable por logs en Azure Web App)
- Auto-scaling (el tráfico real no lo justifica hasta crecer significativamente)
- Bicep / IaC (el portal de Azure es aceptable para un setup inicial si se documenta paso a paso)

**El núcleo no negociable** para tener un sistema funcional y confiable desde el día 1: TDD en Domain + Application, Clean Architecture, JWT, CI/CD básico con GitHub Actions, y el diseño tablet-first.

---

## 8. Modelo de Colaboración y Comunicación

- **Herramienta de seguimiento:** Tablero compartido (Linear / Notion / GitHub Projects — por definir con el cliente).
- **Dinámica de Trabajo (Sábados):** Bloque enfocado de 10:00 a 13:00 hrs para trabajo simultáneo e independiente en lugar de reuniones sincronas, optimizando el tiempo vía subida de cambios asíncrona.
- **Canal de comunicación cotidiana:** WhatsApp o correo electrónico según preferencia del cliente.
- **Revisión de fases:** Al finalizar cada fase se presentará un demo asíncrono o en vivo. La aprobación del cliente desbloquea la siguiente fase.
- **Control de versiones:** Todo el código vive en un repositorio privado de GitHub al que el cliente tendrá acceso completo en todo momento.
- **Ambientes:**
  - `staging` — para pruebas y revisión del cliente, actualizado con cada merge a `develop`.
  - `production` — ambiente vivo, actualizado únicamente al cierre de cada fase con aprobación explícita.

---

## 9. Responsabilidades del Cliente

Para garantizar el avance del proyecto, Talent Institute se compromete a:

| Responsabilidad | Detalle |
|---|---|
| Punto de contacto | Designar una persona con autoridad para tomar decisiones técnicas/funcionales |
| Validación de fases | Revisar y aprobar/rechazar cada entregable en un plazo máximo de **5 días hábiles** |
| Acceso a Azure | Proveer acceso o crear la suscripción de Azure donde se desplegará el sistema |
| Información de negocio | Proveer catálogo de materias, niveles, reglas de méritos/privilegios y cualquier dato inicial necesario |
| Retroalimentación de UX | Participar en al menos una sesión de revisión del frontend por fase (Fases 3 y 4) |
| Onboarding | Asegurar disponibilidad del personal clave (Supervisora, Monitora, Principal) para la sesión de onboarding |

---

## 10. Supuestos

Este SOW se elabora bajo los siguientes supuestos. Si alguno cambia, el alcance deberá revisarse:

1. Se contará con una suscripción activa de Microsoft Azure para el despliegue.
2. El número de alumnos activos no supera los **150 alumnos** en el lanzamiento inicial (arquitectura dimensionada para este volumen).
3. El sistema será utilizado exclusivamente por personal interno del colegio (sin acceso público ni portal de padres en esta versión).
4. Las reglas de negocio del sistema de méritos/privilegios estarán definidas antes del inicio de la Fase 2.
5. No se requiere migración de datos históricos; el sistema arranca con datos limpios.
6. El dominio web y el certificado SSL se gestionan por separado.

**Nota — Costos de infraestructura Azure:** El costo estimado de operación del sistema en Azure es de aproximadamente **$30–80 USD/mes**, dependiendo del tier de Azure SQL y el plan del Web App elegidos. Este costo es independiente del desarrollo y se paga directamente a Microsoft.

---

## 11. Criterios Generales de Aceptación

El sistema se considerará formalmente aceptado cuando:

- [ ] Los tres roles (Principal, Supervisora, Monitora) pueden iniciar sesión y acceder únicamente a las funcionalidades correspondientes a su rol.
- [ ] Una Supervisora puede asignar un PACE a un alumno y registrar avance de puntos.
- [ ] Una Monitora puede registrar la meta diaria de un alumno y otorgar/quitar un mérito.
- [ ] El Dashboard General refleja en tiempo real las alertas de alumnos sin meta registrada.
- [ ] El sistema opera sin errores críticos (HTTP 500, pérdida de datos) durante **5 días hábiles de uso real** en producción.
- [ ] La cobertura de pruebas automatizadas en la capa de Application es ≥ 90%.
- [ ] El pipeline CI/CD despliega automáticamente a producción tras aprobación manual.
- [ ] La documentación de usuario ha sido entregada y revisada.
