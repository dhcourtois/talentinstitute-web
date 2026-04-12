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
  - Restricciones rígidas de formato y tipo de dato (Teclados numéricos nativos cuando sólo se piden números).
  - Alertas instantáneas (toast, o input borders) con colores predefinidos si la calificación de una meta excede los umbrales esperados (ej. más de 100% u objetivos irreales).
  - *Confirmaciones Forzosas* para todo acto destructivo. Eliminar un registro semanal o un PACE entero requerirá escribir una frase confirmatoria o usar botones de prensado sostenido.

## 📱 Responsividad "Tablet First"
- Puesto que tanto Supervisoras como Monitoras rara vez permanecen sentadas o estáticas detrás del escritorio docente, el uso del sistema principal será mediante **Tablets** utilizadas tanto en *Portrait* como en *Landscape*.
- Las áreas de tap táctil (botones, checks) obedecerán al estándar mínimo de 44x44px. Elementos como los menús de navegación deben comportarse adecuadamente al agarre a dos manos (menús laterales fácilmente alcanzables por los pulgares o barras inferioes navbars para la navegación rápida).

## 🧩 Componentes y Vistas Principales

### Dashboard General (Supervisora)
- **Centro de Mando Académico:** Una interfaz enfocada en el flujo total del salón.
- **Alertas Prioritarias:** Indicadores instantáneos de alumnos atascados ("Stuck"), o PACEs que urgen evaluación final en "Testing Box".
- **Análisis Estadístico Breve:** Gráficos de tendencias semanales generalizadas.

### Vista de Perfil de Alumno (Monitora)
- **Velocidad y Precisión:** Diseñada para permitir el ciclo continuo y repetitivo por el escritorio de cada alumno de manera ágil.
- **Header Identificativo:** Avatar del niño, estatus del día y el widget vital del control numérico de Méritos.
- **Gestor Rápido de Metas:** Matriz o formulario ágil de los PACEs actuales, optimizado para checar "Metas Terminadas", estatus correctos, y puntajes ("Scores") del día actual. Acciones con respuesta a un par de "Taps".
