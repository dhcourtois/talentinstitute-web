# Especificación de Interfaz de Usuario (Frontend) - Talent Institute

Este documento define la estructura visual, interactiva y la topología del Frontend del sistema. Aplicaremos una arquitectura de **Single Page Application (SPA)**, que se comunicará mediante llamadas asíncronas de API al backend .NET 8 y estará servida y optimizada mediante los servicios de Azure Web Apps.

## 🎨 Estética y Diseño (Estilo Apple)
Buscamos un diseño premium que refleje profesionalismo, innovación y paz mental en el ámbito educativo, adoptando las directrices de la capa superior de interfaces de diseño contemporáneas:
- **Espacio en Blanco Generoso (Negative Space):** Elementos separados intencionalmente para evitar la saturación visual. Esto disminuye la carga cognitiva en operarios como la monitora, quien gestiona decenas de alumnos al día.
- **Tipografía y Legibilidad:** Apuntalado en una familia sans-serif realista: primando el uso de la fuente **San Francisco (SF Pro)** o usando **Manrope** como su alternativa principal para la web. La jerarquía se marcará con variaciones claras de grosor tipográfico (light, regular, bold), no con excesiva variación en tamaños o colores.
- **Jerarquía Visual y Contraste:** Uso de fondos translúcidos sutiles, esquilas suavemente redondeadas (`border-radius`), sombras proyectadas suaves, y una paleta monocromática interrumpida solo por el "color de acento" para llamadas principales a la acción (CTAs).

## 🧠 Heurísticas de Nielsen en la Interfaz Educativa (ACE)

Una interfaz debe ser tan inteligente como la pedagogía que sustenta. Para garantizar evaluaciones ágiles, aplicaremos rigurosamente:

### 1. Reconocimiento antes que recuerdo (Recognition rather than recall)
- **El Problema:** La supervisora no debería tener que usar su memoria a corto plazo para recordar qué metas dejó aprobadas la semana pasada, o cómo se califica el PACE específico que está observando de un alumno.
- **La Implementación:** Durante la captura de los avances o scoring, los campos estarán acompañados (como metadatos *inline*) del historial de los últimos días del alumno para ese módulo. Las escalas de calificación, referencias de evaluación y el histórico visual servirán de ancla, brindando *"el contexto justo a tiempo"* sin obligar al usuario a cambiar de pestaña o pantalla para recordarlo.

### 2. Prevención de errores (Error prevention)
- **El Problema:** Al ingresar calificaciones o avances, es común el error de "dedo gordo" (el uso de tablets táctiles fomenta el desliz o error por prisa).
- **La Implementación:**
  - Restricciones rígidas de formato y tipo de dato (teclados numéricos nativos cuando solo se piden números).
  - Alertas instantáneas (toast o input borders en rojo) si la calificación de una meta excede los umbrales esperados (ej. más de 50 puntos en un PACE, o metas con fecha futura).
  - *Confirmaciones forzosas* para todo acto destructivo. Eliminar un registro semanal o revocar un mérito requerirá un diálogo de confirmación explícita con descripción de la consecuencia.

### 3. Visibilidad del estado del sistema (Visibility of system status)
- **El Problema:** La monitora no debe quedarse con la duda de si su acción (registrar una meta, otorgar un mérito) fue guardada o no.
- **La Implementación:**
  - Todo botón de acción muestra un estado de carga visual (spinner inline, botón deshabilitado) mientras la petición API está en vuelo.
  - Confirmación de éxito mediante toast no intrusivo (esquina superior derecha, 3 segundos, verde) inmediatamente tras cada operación exitosa.
  - Indicador de conexión perdida si el API no responde en >5 segundos: banner superior discreto "Sin conexión — los cambios no se están guardando".

### 4. Correspondencia con el mundo real (Match between system and the real world)
- **El Problema:** El personal del colegio conoce términos del sistema ACE, no jerga técnica de software.
- **La Implementación:**
  - Usar vocabulario ACE en toda la interfaz: "PACE" (no "módulo"), "Score Station" (no "calificación final"), "Meta" (no "tarea"), "Mérito/Demérito" (no "punto positivo/negativo").
  - Los estados de progreso usan lenguaje natural: "En progreso", "Listo para Score Station", "Completado" — nunca códigos internos como `InProgress`, `SelfTest`, `FinalTestPassed`.
  - Los íconos son reconocibles sin etiqueta (estrella = mérito, check = meta cumplida, campana = alerta).

### 5. Control y libertad del usuario (User control and freedom)
- **El Problema:** Un toque accidental en tablet puede registrar un demérito o marcar una meta como completada por error.
- **La Implementación:**
  - Opción de **deshacer** (Undo, 5 segundos) después de cualquier acción reversible, presentada como acción inline en el toast de confirmación: "Meta marcada ✓  —  Deshacer".
  - El flujo nunca atrapa al usuario: toda vista tiene navegación de regreso visible; ningún modal es obligatorio para continuar usando el sistema.
  - Las acciones de revocación de méritos (destructivas) están disponibles pero requieren el rol correcto (Supervisora / Principal).

### 6. Consistencia y estándares (Consistency and standards)
- **El Problema:** Un sistema usado por múltiples personas en paralelo debe comportarse de forma predecible en cada pantalla.
- **La Implementación:**
  - **Design tokens** centralizados en un archivo CSS de variables (`--color-accent`, `--radius-md`, `--shadow-card`, etc.) — ningún valor de color, sombra o espaciado se repite hardcodeado en componentes individuales.
  - Los badges de estado siempre usan la misma paleta semántica en todo el sistema: verde = positivo/completado, naranja = advertencia/pendiente, rojo = error/en riesgo, azul = neutral/informativo.
  - Los botones de acción primaria siempre están en la misma posición relativa dentro de sus contenedores (esquina superior derecha en headers de sección).

### 7. Reconocimiento, diagnóstico y recuperación ante errores (Help users recognize, diagnose, and recover from errors)
- **El Problema:** Cuando algo falla (API caída, validación rechazada), el usuario no debe ver un mensaje genérico "Error 500".
- **La Implementación:**
  - Los mensajes de error explican **qué salió mal** y **qué hacer**: "No se pudo guardar la meta. Verifica que el PACE esté activo e intenta de nuevo."
  - Los errores de validación se muestran inline junto al campo que los causó, nunca en un alert del navegador.
  - Si una operación falla por pérdida de conexión, el sistema ofrece reintentar con un botón explícito en lugar de dejar la pantalla en estado roto.

### 8. Flexibilidad y eficiencia de uso (Flexibility and efficiency of use)
- **El Problema:** La monitora realiza el mismo flujo docenas de veces al día (revisar meta → registrar puntos → siguiente alumno). Cada segundo extra se multiplica.
- **La Implementación:**
  - La Vista de Perfil del Alumno está optimizada para el flujo de la monitora: el checklist de metas y el registro de puntos son los primeros elementos visibles, sin necesidad de scroll.
  - Accesos directos de teclado en desktop para las acciones más frecuentes (documentados en tooltip al hacer hover).
  - El Dashboard permite navegar al perfil de cualquier alumno con un solo toque desde la tabla de progreso.
  - Los formularios de registro rápido (mérito, meta) pre-llenan la fecha actual y el alumno en contexto; el usuario solo ingresa lo que varía.

### 9. Diseño estético y minimalista (Aesthetic and minimalist design)
- **El Problema:** Una interfaz sobrecargada aumenta el tiempo de procesamiento visual y el riesgo de error, especialmente en dispositivos táctiles.
- **La Implementación:**
  - Cada vista expone únicamente la información necesaria para la tarea en curso. Los datos secundarios (historial extenso, metadatos técnicos) viven en secciones expandibles o en pantallas de detalle.
  - Paleta de color restringida a tokens definidos. El "color de acento" (`#0071E3`) se reserva exclusivamente para la acción primaria de cada pantalla; no se usa decorativamente.
  - Sin bordes decorativos, gradientes complejos ni animaciones de entrada en elementos funcionales. Las animaciones se limitan a transiciones de estado (hover, focus, loading).

### 10. Ayuda y documentación (Help and documentation)
- **El Problema:** El personal nuevo (o reemplazos) debe poder aprender el sistema sin depender de una persona que les explique.
- **La Implementación:**
  - Tooltips contextuales (ícono `?` pequeño junto a términos técnicos ACE) que explican el concepto en 1-2 líneas sin abandonar la pantalla.
  - Pantalla de "primera vez" para cada rol: al hacer login por primera vez, un overlay guiado (3-4 pasos) señala los elementos clave de la interfaz correspondiente a su rol.
  - Documentación de usuario por rol entregada como Fase 4 (ver SOW), accesible desde el menú de configuración del sistema.

## 📱 Responsividad "Tablet First"
- Puesto que tanto Supervisoras como Monitoras rara vez permanecen sentadas o estáticas detrás del escritorio docente, el uso del sistema principal será mediante **Tablets** utilizadas tanto en *Portrait* como en *Landscape*.
- Las áreas de tap táctil (botones, checks) obedecerán al estándar mínimo de 44x44px. Elementos como los menús de navegación deben comportarse adecuadamente al agarre a dos manos (menús laterales fácilmente alcanzables por los pulgares o barras inferioes navbars para la navegación rápida).

## 🧩 Componentes y Vistas Principales

### Dashboard General (Supervisora / Principal)
- **Centro de Mando Académico:** Una interfaz enfocada en el flujo total del salón.
- **Alertas Prioritarias:** Indicadores instantáneos de alumnos sin meta 2+ días y Paces listos para Auto-Test.
- **Análisis Estadístico Breve:** Gráfica de barras de metas cumplidas por día en la semana actual.

### Vista de Perfil de Alumno (Monitora / Supervisora / Principal)
- **Velocidad y Precisión:** Diseñada para el ciclo repetitivo de la monitora recorriendo escritorios.
- **Header Identificativo:** Avatar, nombre, grado, balance de méritos y chips de privilegios activos/inactivos.
- **Gestor Rápido de Metas:** Checklist por turno (Mañana/Tarde) con toggle de estado y campo de puntaje inline para Score Station.

---

## 🗺 Matriz Vista → Acciones → API

Referencia operacional para implementación. Cada acción de UI se mapea a un endpoint concreto y al rol mínimo requerido.

### Vista: Login (`/login`)

| Acción | Endpoint | Rol |
|---|---|---|
| Enviar credenciales | `POST /api/v1/Auth/login` | Todos |
| Redirigir post-login | Router Angular (sin API) | — |

### Vista: Dashboard General (`/dashboard`)

| Acción | Endpoint | Rol mínimo |
|---|---|---|
| Cargar métricas (4 tarjetas) | `GET /api/v1/Dashboard/resumen` | Supervisora |
| Cargar banner de alertas | `GET /api/v1/Dashboard/alertas` | Supervisora |
| Cargar tabla de alumnos | `GET /api/v1/Alumnos` | Supervisora |
| Navegar a perfil de alumno | Router Angular (sin API) | — |
| Cargar panel Score Station pendiente | `GET /api/v1/Paces/alumno/{id}` (filtrado por estado) | Supervisora |

### Vista: Perfil de Alumno (`/alumnos/:id`)

| Acción | Endpoint | Rol mínimo |
|---|---|---|
| Cargar datos del alumno | `GET /api/v1/Alumnos/{alumnoId}` | Monitora |
| Cargar Paces activos | `GET /api/v1/Paces/alumno/{alumnoId}` | Monitora |
| Cargar metas de la semana | `GET /api/v1/Progreso/{alumnoId}/semana/{fechaInicio}` | Monitora |
| Marcar meta como Completada | `PUT /api/v1/Progreso/metas/{metaId}/estatus` `{ estado: "Completada" }` | Monitora |
| Marcar meta como Rechazada | `PUT /api/v1/Progreso/metas/{metaId}/estatus` `{ estado: "Rechazada" }` | Monitora |
| Registrar puntos Score Station | `PUT /api/v1/Progreso/metas/{metaId}/estatus` `{ estado: "Scored", puntajeObtenido: n }` | Supervisora |
| Agregar meta nueva | `POST /api/v1/Progreso` | Supervisora |
| Otorgar mérito | `POST /api/v1/Meritos` `{ tipo: "Merito", ... }` | Monitora |
| Registrar demérito | `POST /api/v1/Meritos` `{ tipo: "Demerito", ... }` | Monitora |
| Cargar historial de méritos | `GET /api/v1/Meritos/alumno/{alumnoId}` | Monitora |
| Revocar un mérito | `PATCH /api/v1/Meritos/{meritoId}/revocar` | Supervisora |
| Asignar nuevo Pace al alumno | `POST /api/v1/Paces/asignar` | Supervisora |

### Vista: Gestión de Staff (`/staff`) — Solo Principal

| Acción | Endpoint | Rol mínimo |
|---|---|---|
| Cargar lista de usuarios | `GET /api/v1/Staff` | Principal |
| Crear nuevo usuario | `POST /api/v1/Staff` | Principal |
| Editar usuario / cambiar rol | `PUT /api/v1/Staff/{staffId}` | Principal |
| Desactivar acceso | `PATCH /api/v1/Staff/{staffId}/desactivar` | Principal |

### Vista: Configuración de Privilegios (`/configuracion/privilegios`) — Solo Principal

| Acción | Endpoint | Rol mínimo |
|---|---|---|
| Cargar umbrales actuales | `GET /api/v1/Configuracion/privilegios` | Principal |
| Guardar nuevos umbrales | `PUT /api/v1/Configuracion/privilegios` | Principal |
