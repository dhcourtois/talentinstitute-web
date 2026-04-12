# Constitución del Sistema Institucional - Talent Institute

## 🎯 Misión
Digitalizar el progreso individual de los alumnos (PACEs) y llevar un estricto control de méritos.

## 🏗 Principios de Arquitectura y Desarrollo
- **Arquitectura Limpia (Clean Architecture):** Separación de responsabilidades y bajo acoplamiento para garantizar la escalabilidad y mantenibilidad.
- **Principios SOLID:** Diseño orientado a objetos robusto, flexible y mantenible.
- **TDD (Test Driven Development):** Su uso es **obligatorio**. Las pruebas deben guiar y validar todo el desarrollo desde su concepción.

## 🎨 Experiencia de Usuario (UX)
- **Interfaz:** Diseño minimalista tipo Apple (uso de espacios en blanco, tipografía clara, bordes redondeados, paleta de colores sobria).
- **Usabilidad:** El sistema debe cumplir estrictamente con las **10 Heurísticas de Jakob Nielsen**.

## 🛠 Stack Tecnológico

| Capa | Decisión |
|---|---|
| **Backend / Framework** | C# / ASP.NET Core 8 — Clean Architecture |
| **Base de Datos** | Azure SQL Database (EF Core Code-First) |
| **Frontend / SPA** | **Angular 18** + **TypeScript** |
| **Estilos (Frontend)** | CSS por componente (Angular component styles) — sin framework de UI externo; diseño propio tipo Apple |
| **Cliente HTTP (Frontend)** | `HttpClient` nativo de Angular con interceptor JWT |
| **Despliegue / Cloud** | Azure Web Apps (Linux PaaS) |
| **IaC** | Azure Bicep |
| **CI/CD** | GitHub Actions |

### Justificación del stack frontend
Angular 18 se elige por:
- **Alineación con el backend**: el modelo mental de Angular (inyección de dependencias, decoradores, servicios, módulos) es prácticamente idéntico al de ASP.NET Core, lo que reduce la fricción al trabajar en ambas capas.
- **Batería incluida**: `HttpClient` con interceptores (JWT adjuntado en un solo lugar), `Router` con `CanActivate` guards para control de roles, y `ReactiveForms` para validación — todo sin dependencias externas adicionales.
- **TypeScript estricto de fábrica**: los contratos entre la API REST y los componentes se validan en tiempo de compilación.
- Sin framework de UI externo para mantener el diseño Apple minimalista definido en `frontend-ui.md` sin interferencias de estilos de terceros.

## ⚖️ Regla de Oro
> **"La especificación es la única fuente de verdad."**
