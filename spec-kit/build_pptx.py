from pptx import Presentation
from pptx.util import Inches, Pt, Emu
from pptx.dml.color import RGBColor
from pptx.enum.text import PP_ALIGN
from pptx.util import Inches, Pt
import copy

# ── COLORS ──────────────────────────────────────────────
C_DARK    = RGBColor(0x1D, 0x1D, 0x1F)   # #1D1D1F
C_MID     = RGBColor(0x6E, 0x6E, 0x73)   # #6E6E73
C_LIGHT   = RGBColor(0xF5, 0xF5, 0xF7)   # #F5F5F7
C_WHITE   = RGBColor(0xFF, 0xFF, 0xFF)
C_ACCENT  = RGBColor(0x00, 0x71, 0xE3)   # #0071E3
C_SUCCESS = RGBColor(0x34, 0xC7, 0x59)   # #34C759
C_WARN    = RGBColor(0xFF, 0x95, 0x00)   # #FF9500
C_DANGER  = RGBColor(0xFF, 0x3B, 0x30)   # #FF3B30
C_PURPLE  = RGBColor(0x5E, 0x5C, 0xE6)   # #5E5CE6
C_NAVY    = RGBColor(0x00, 0x2D, 0x72)   # Deep navy for title slide

SLIDE_W = Inches(13.33)
SLIDE_H = Inches(7.5)

prs = Presentation()
prs.slide_width  = SLIDE_W
prs.slide_height = SLIDE_H

blank_layout = prs.slide_layouts[6]  # completely blank

def add_rect(slide, x, y, w, h, fill_rgb=None, line_rgb=None, line_pt=0):
    shape = slide.shapes.add_shape(1, Inches(x), Inches(y), Inches(w), Inches(h))
    if fill_rgb:
        shape.fill.solid()
        shape.fill.fore_color.rgb = fill_rgb
    else:
        shape.fill.background()
    if line_rgb and line_pt > 0:
        shape.line.color.rgb = line_rgb
        shape.line.width = Pt(line_pt)
    else:
        shape.line.fill.background()
    return shape

def add_text(slide, text, x, y, w, h,
             font_size=14, bold=False, color=None, align=PP_ALIGN.LEFT,
             italic=False, wrap=True):
    txb = slide.shapes.add_textbox(Inches(x), Inches(y), Inches(w), Inches(h))
    txb.word_wrap = wrap
    tf = txb.text_frame
    tf.word_wrap = wrap
    p = tf.paragraphs[0]
    p.alignment = align
    run = p.add_run()
    run.text = text
    run.font.size = Pt(font_size)
    run.font.bold = bold
    run.font.italic = italic
    run.font.color.rgb = color or C_DARK
    return txb

def add_multiline(slide, lines, x, y, w, h, font_size=13, color=None, line_spacing=1.2):
    """lines: list of (text, bold, color_override)"""
    txb = slide.shapes.add_textbox(Inches(x), Inches(y), Inches(w), Inches(h))
    txb.word_wrap = True
    tf = txb.text_frame
    tf.word_wrap = True
    for i, item in enumerate(lines):
        if isinstance(item, str):
            text, bold, col = item, False, color or C_DARK
        else:
            text, bold, col = item[0], item[1] if len(item)>1 else False, item[2] if len(item)>2 else (color or C_DARK)
        p = tf.paragraphs[0] if i == 0 else tf.add_paragraph()
        p.alignment = PP_ALIGN.LEFT
        run = p.add_run()
        run.text = text
        run.font.size = Pt(font_size)
        run.font.bold = bold
        run.font.color.rgb = col
    return txb

def slide_counter(slide, n, total=13):
    add_text(slide, f"{n} / {total}", 12.2, 7.1, 1.0, 0.3,
             font_size=9, color=C_MID, align=PP_ALIGN.RIGHT)

# ═══════════════════════════════════════════════════════
# SLIDE 1 — PORTADA
# ═══════════════════════════════════════════════════════
sl = prs.slides.add_slide(blank_layout)

# Full dark background
add_rect(sl, 0, 0, 13.33, 7.5, fill_rgb=C_DARK)

# Left accent strip
add_rect(sl, 0, 0, 0.06, 7.5, fill_rgb=C_ACCENT)

# Blue accent block top-right
add_rect(sl, 10.5, 0, 2.83, 2.2, fill_rgb=C_ACCENT)

# Title
add_text(sl, "Sistema Institucional", 0.7, 1.8, 9.0, 1.0,
         font_size=44, bold=True, color=C_WHITE)
add_text(sl, "Talent Institute", 0.7, 2.9, 9.0, 0.7,
         font_size=28, bold=False, color=RGBColor(0xA0,0xC8,0xFF))
add_text(sl, "Digitalización del modelo ACE · Mérida, Yucatán", 0.7, 3.65, 9.0, 0.5,
         font_size=16, color=RGBColor(0x99,0x99,0xAA))

# Divider line
add_rect(sl, 0.7, 4.3, 5.0, 0.03, fill_rgb=RGBColor(0x44,0x44,0x55))

# Tech tags
for i, tag in enumerate(["ASP.NET Core 8", "Angular 18", "Azure SQL", "Clean Architecture"]):
    add_rect(sl, 0.7 + i*2.6, 4.5, 2.3, 0.38,
             fill_rgb=RGBColor(0x2A,0x2A,0x3A), line_rgb=RGBColor(0x55,0x55,0x77), line_pt=0.5)
    add_text(sl, tag, 0.8 + i*2.6, 4.53, 2.1, 0.3,
             font_size=11, bold=True, color=RGBColor(0xAA,0xCC,0xFF), align=PP_ALIGN.CENTER)

add_text(sl, "Abril 2026", 0.7, 6.9, 3.0, 0.4, font_size=12, color=RGBColor(0x66,0x66,0x77))


# ═══════════════════════════════════════════════════════
# SLIDE 2 — ¿QUÉ ES TALENT INSTITUTE?
# ═══════════════════════════════════════════════════════
sl = prs.slides.add_slide(blank_layout)
add_rect(sl, 0, 0, 13.33, 7.5, fill_rgb=C_LIGHT)
add_rect(sl, 0, 0, 13.33, 1.1, fill_rgb=C_WHITE)
add_rect(sl, 0, 1.1, 13.33, 0.04, fill_rgb=RGBColor(0xD2,0xD2,0xD7))

add_text(sl, "¿Qué es Talent Institute?", 0.6, 0.2, 10.0, 0.75,
         font_size=26, bold=True, color=C_DARK)

# Left card
add_rect(sl, 0.5, 1.4, 5.8, 5.6, fill_rgb=C_WHITE, line_rgb=RGBColor(0xD2,0xD2,0xD7), line_pt=0.5)
add_text(sl, "El Colegio", 0.85, 1.65, 5.0, 0.45, font_size=13, bold=True, color=C_MID)
add_multiline(sl, [
    ("Instituto Cristiano en Los Héroes", True, C_DARK),
    ("Calle 83 No. 492 · Mérida, Yucatán", False, C_MID),
    ("", False, C_MID),
    ("Primaria · Secundaria · Preparatoria", True, C_DARK),
    ("Menos de 150 alumnos activos", False, C_MID),
    ("Personal: Principal, Supervisoras, Monitoras", False, C_MID),
], 0.85, 2.2, 5.1, 3.5, font_size=14)

# Right card — ACE system
add_rect(sl, 6.9, 1.4, 5.9, 5.6, fill_rgb=C_WHITE, line_rgb=RGBColor(0xD2,0xD2,0xD7), line_pt=0.5)
add_text(sl, "Sistema ACE", 7.2, 1.65, 5.0, 0.45, font_size=13, bold=True, color=C_MID)
ace_items = [
    "📘  PACEs — módulos de autoaprendizaje por materia",
    "🎯  Metas diarias fijadas por el alumno cada mañana",
    "📊  Score Station — auto-evaluación supervisada",
    "⭐  Méritos y deméritos → privilegios activos",
    "🚀  Progresión individual, sin avance grupal",
]
for i, item in enumerate(ace_items):
    add_text(sl, item, 7.2, 2.25 + i*0.78, 5.3, 0.6, font_size=13, color=C_DARK)

slide_counter(sl, 2)


# ═══════════════════════════════════════════════════════
# SLIDE 3 — EL RETO
# ═══════════════════════════════════════════════════════
sl = prs.slides.add_slide(blank_layout)
add_rect(sl, 0, 0, 13.33, 7.5, fill_rgb=C_LIGHT)
add_rect(sl, 0, 0, 13.33, 1.1, fill_rgb=C_WHITE)
add_rect(sl, 0, 1.1, 13.33, 0.04, fill_rgb=RGBColor(0xD2,0xD2,0xD7))
add_text(sl, "El reto operativo actual", 0.6, 0.2, 10.0, 0.75,
         font_size=26, bold=True, color=C_DARK)

problems = [
    ("Sin visibilidad en tiempo real", "La Supervisora no sabe qué alumnos están estancados hasta revisar manualmente cada escritorio.", C_DANGER),
    ("Metas y scoring en papel", "Los registros diarios se llevan en hojas físicas, propensos a errores, pérdidas y difícil análisis histórico.", C_WARN),
    ("Méritos sin trazabilidad", "No existe un historial auditado de quién otorgó cada mérito o demérito ni cuándo.", C_WARN),
    ("Privilegios sin automatización", "El cálculo de qué privilegios tiene cada alumno se hace a juicio, no con reglas claras y consistentes.", C_DANGER),
]
for i, (title, desc, accent) in enumerate(problems):
    x = 0.5 + (i % 2) * 6.4
    y = 1.5 + (i // 2) * 2.7
    add_rect(sl, x, y, 6.0, 2.4, fill_rgb=C_WHITE, line_rgb=RGBColor(0xD2,0xD2,0xD7), line_pt=0.5)
    add_rect(sl, x, y, 0.07, 2.4, fill_rgb=accent)
    add_text(sl, title, x+0.25, y+0.2, 5.5, 0.5, font_size=14, bold=True, color=C_DARK)
    add_text(sl, desc, x+0.25, y+0.75, 5.5, 1.4, font_size=12, color=C_MID)

slide_counter(sl, 3)


# ═══════════════════════════════════════════════════════
# SLIDE 4 — LA SOLUCIÓN
# ═══════════════════════════════════════════════════════
sl = prs.slides.add_slide(blank_layout)
add_rect(sl, 0, 0, 13.33, 7.5, fill_rgb=C_DARK)
add_rect(sl, 0, 0, 0.06, 7.5, fill_rgb=C_ACCENT)

add_text(sl, "La solución", 0.7, 0.5, 10.0, 0.6, font_size=18, color=RGBColor(0x99,0x99,0xAA))
add_text(sl, "Una aplicación web institucional", 0.7, 1.1, 11.0, 0.9,
         font_size=36, bold=True, color=C_WHITE)

add_text(sl, "Tablet-first · 3 roles · Tiempo real · TDD · Azure", 0.7, 2.1, 11.0, 0.5,
         font_size=16, color=RGBColor(0x00,0x99,0xFF))

pillars = [
    ("👁", "Visibilidad\ncompleta", "Dashboard en tiempo real con alertas automáticas de alumnos estancados"),
    ("⭐", "Méritos\nauditados", "Bitácora trazable con cálculo automático de privilegios por reglas configurables"),
    ("📊", "Progreso\ndigitalizado", "PACEs, metas diarias y Score Station registrados y consultables desde cualquier tablet"),
    ("🔐", "Roles y\nseguridad", "JWT stateless con tres roles: Principal, Supervisora y Monitora"),
]
for i, (icon, title, desc) in enumerate(pillars):
    x = 0.5 + i * 3.2
    add_rect(sl, x, 2.9, 3.0, 3.8, fill_rgb=RGBColor(0x2A,0x2A,0x3A), line_rgb=RGBColor(0x44,0x44,0x55), line_pt=0.5)
    add_text(sl, icon, x+0.2, 3.05, 2.6, 0.5, font_size=24, align=PP_ALIGN.CENTER)
    add_text(sl, title, x+0.2, 3.6, 2.6, 0.55, font_size=14, bold=True, color=C_WHITE, align=PP_ALIGN.CENTER)
    add_text(sl, desc, x+0.2, 4.25, 2.6, 2.0, font_size=11, color=RGBColor(0xAA,0xAA,0xBB), align=PP_ALIGN.CENTER)

slide_counter(sl, 4)


# ═══════════════════════════════════════════════════════
# SLIDE 5 — TRES ROLES
# ═══════════════════════════════════════════════════════
sl = prs.slides.add_slide(blank_layout)
add_rect(sl, 0, 0, 13.33, 7.5, fill_rgb=C_LIGHT)
add_rect(sl, 0, 0, 13.33, 1.1, fill_rgb=C_WHITE)
add_rect(sl, 0, 1.1, 13.33, 0.04, fill_rgb=RGBColor(0xD2,0xD2,0xD7))
add_text(sl, "Tres roles, una herramienta", 0.6, 0.2, 10.0, 0.75,
         font_size=26, bold=True, color=C_DARK)

roles = [
    ("Principal", C_PURPLE, "👤 Administrador total",
     ["Gestión de usuarios de staff", "Configuración de umbrales de privilegios", "Acceso al Dashboard General", "Alta de PACEs en el catálogo"]),
    ("Supervisora", C_ACCENT, "📚 Autoridad académica",
     ["Asignación de PACEs a alumnos", "Validación de metas y Score Station", "Otorgar / revocar méritos", "Supervisión del dashboard y alertas"]),
    ("Monitora", C_SUCCESS, "✅ Operación diaria",
     ["Registro de metas completadas", "Calificación operativa (scoring)", "Otorgar méritos y deméritos rápidos", "Consulta de perfil del alumno"]),
]
for i, (name, color, subtitle, perms) in enumerate(roles):
    x = 0.5 + i * 4.3
    add_rect(sl, x, 1.4, 4.0, 5.7, fill_rgb=C_WHITE, line_rgb=RGBColor(0xD2,0xD2,0xD7), line_pt=0.5)
    add_rect(sl, x, 1.4, 4.0, 0.6, fill_rgb=color)
    add_text(sl, name, x+0.2, 1.5, 3.6, 0.45, font_size=18, bold=True, color=C_WHITE)
    add_text(sl, subtitle, x+0.2, 2.1, 3.6, 0.4, font_size=13, bold=True, color=color)
    for j, perm in enumerate(perms):
        add_rect(sl, x+0.2, 2.65+j*0.78, 0.06, 0.38, fill_rgb=color)
        add_text(sl, perm, x+0.4, 2.6+j*0.78, 3.4, 0.5, font_size=12, color=C_DARK)

slide_counter(sl, 5)


# ═══════════════════════════════════════════════════════
# SLIDE 6 — MÓDULOS DEL SISTEMA
# ═══════════════════════════════════════════════════════
sl = prs.slides.add_slide(blank_layout)
add_rect(sl, 0, 0, 13.33, 7.5, fill_rgb=C_LIGHT)
add_rect(sl, 0, 0, 13.33, 1.1, fill_rgb=C_WHITE)
add_rect(sl, 0, 1.1, 13.33, 0.04, fill_rgb=RGBColor(0xD2,0xD2,0xD7))
add_text(sl, "Módulos del sistema", 0.6, 0.2, 10.0, 0.75,
         font_size=26, bold=True, color=C_DARK)

modules = [
    ("🔐", "Autenticación", "JWT · 3 roles · Claims-based", C_DARK),
    ("👩‍🎓", "Alumnos", "Alta, perfil, privilegios", C_ACCENT),
    ("📘", "PACEs", "Catálogo, asignación, ciclo de vida", C_ACCENT),
    ("🎯", "Metas", "Registro diario por turno", C_SUCCESS),
    ("⭐", "Méritos", "Bitácora auditable, revocación", C_WARN),
    ("📊", "Dashboard", "Métricas, alertas, tendencias", C_PURPLE),
    ("👥", "Staff", "Gestión de usuarios internos", C_MID),
]
for i, (icon, name, desc, color) in enumerate(modules):
    x = 0.5 + (i % 4) * 3.1
    y = 1.5 + (i // 4) * 2.85
    add_rect(sl, x, y, 2.8, 2.55, fill_rgb=C_WHITE, line_rgb=RGBColor(0xD2,0xD2,0xD7), line_pt=0.5)
    add_rect(sl, x, y, 2.8, 0.07, fill_rgb=color)
    add_text(sl, icon, x+0.2, y+0.2, 2.4, 0.5, font_size=22, align=PP_ALIGN.CENTER)
    add_text(sl, name, x+0.1, y+0.75, 2.6, 0.45, font_size=14, bold=True, color=C_DARK, align=PP_ALIGN.CENTER)
    add_text(sl, desc, x+0.1, y+1.2, 2.6, 0.8, font_size=11, color=C_MID, align=PP_ALIGN.CENTER)

slide_counter(sl, 6)


# ═══════════════════════════════════════════════════════
# SLIDE 7 — DASHBOARD (SUPERVISORA)
# ═══════════════════════════════════════════════════════
sl = prs.slides.add_slide(blank_layout)
add_rect(sl, 0, 0, 13.33, 7.5, fill_rgb=C_LIGHT)
add_rect(sl, 0, 0, 13.33, 1.1, fill_rgb=C_WHITE)
add_rect(sl, 0, 1.1, 13.33, 0.04, fill_rgb=RGBColor(0xD2,0xD2,0xD7))
add_text(sl, "Dashboard General — Vista Supervisora", 0.6, 0.2, 10.0, 0.75,
         font_size=26, bold=True, color=C_DARK)
add_text(sl, "Ver mock: mock/dashboard.html", 10.5, 0.35, 2.7, 0.4,
         font_size=10, color=C_ACCENT, align=PP_ALIGN.RIGHT)

# 4 metric cards
metrics = [("24", "Alumnos activos", C_ACCENT), ("18", "Metas hoy", C_SUCCESS), ("7", "PACEs revisión", C_WARN), ("3", "Alertas", C_DANGER)]
for i, (val, lbl, color) in enumerate(metrics):
    x = 0.5 + i * 3.0
    add_rect(sl, x, 1.4, 2.7, 1.3, fill_rgb=C_WHITE, line_rgb=RGBColor(0xD2,0xD2,0xD7), line_pt=0.5)
    add_text(sl, val, x+0.2, 1.55, 2.3, 0.65, font_size=36, bold=True, color=color, align=PP_ALIGN.CENTER)
    add_text(sl, lbl, x+0.1, 2.15, 2.5, 0.4, font_size=11, color=C_MID, align=PP_ALIGN.CENTER)

# Features list
feats = [
    ("🚨", "Banner de alertas automático", "Muestra alumnos con 2+ días sin meta registrada, calculado en tiempo real sin job periódico."),
    ("📋", "Tabla de progreso semanal", "Avance de PACEs, estado de meta del día, balance de méritos y badge de estado por alumno."),
    ("📈", "Gráfica semanal de metas", "Visualización de metas completadas por día en la semana actual, desglosadas por turno."),
    ("⏳", "Score Station pendiente", "Panel lateral con PACEs listos para Auto-Test ordenados por porcentaje de avance."),
]
for i, (icon, title, desc) in enumerate(feats):
    y = 2.95 + i * 1.1
    add_rect(sl, 0.5, y, 0.5, 0.75, fill_rgb=C_LIGHT)
    add_text(sl, icon, 0.6, y+0.1, 0.35, 0.5, font_size=16, align=PP_ALIGN.CENTER)
    add_text(sl, title, 1.2, y+0.0, 11.5, 0.38, font_size=13, bold=True, color=C_DARK)
    add_text(sl, desc,  1.2, y+0.38, 11.5, 0.4, font_size=11, color=C_MID)

slide_counter(sl, 7)


# ═══════════════════════════════════════════════════════
# SLIDE 8 — PERFIL DEL ALUMNO (MONITORA)
# ═══════════════════════════════════════════════════════
sl = prs.slides.add_slide(blank_layout)
add_rect(sl, 0, 0, 13.33, 7.5, fill_rgb=C_LIGHT)
add_rect(sl, 0, 0, 13.33, 1.1, fill_rgb=C_WHITE)
add_rect(sl, 0, 1.1, 13.33, 0.04, fill_rgb=RGBColor(0xD2,0xD2,0xD7))
add_text(sl, "Perfil del Alumno — Vista Monitora", 0.6, 0.2, 10.0, 0.75,
         font_size=26, bold=True, color=C_DARK)
add_text(sl, "Ver mock: mock/student.html", 10.5, 0.35, 2.7, 0.4,
         font_size=10, color=C_ACCENT, align=PP_ALIGN.RIGHT)

sections = [
    ("Hero card", C_ACCENT, "Nombre, grado, fecha de ingreso y chips de privilegios activos/inactivos con colores semánticos."),
    ("PACEs activos", C_ACCENT, "Tarjetas por materia con barra de progreso, estado canónico (EnProgreso, Listo para Auto-Test) y acciones contextuales."),
    ("Checklist de metas", C_SUCCESS, "Metas del día separadas por turno Mañana/Tarde. Toggle de estado inline. Muestra Aprobada, Scored, Pendiente."),
    ("Acciones rápidas", C_WARN, "Otorgar mérito, registrar demérito, actualizar puntos. Cada acción abre un modal de confirmación antes de ejecutar."),
    ("Log de méritos", C_PURPLE, "Últimos 10 registros con motivo, responsable y fecha. Los méritos revocados se muestran tachados con indicador visual."),
]
for i, (title, color, desc) in enumerate(sections):
    y = 1.5 + i * 1.15
    add_rect(sl, 0.5, y, 0.07, 0.75, fill_rgb=color)
    add_text(sl, title, 0.75, y+0.0, 3.5, 0.38, font_size=13, bold=True, color=C_DARK)
    add_text(sl, desc,  0.75, y+0.38, 11.8, 0.4, font_size=11, color=C_MID)

slide_counter(sl, 8)


# ═══════════════════════════════════════════════════════
# SLIDE 9 — FLUJO DE ESTADOS (META)
# ═══════════════════════════════════════════════════════
sl = prs.slides.add_slide(blank_layout)
add_rect(sl, 0, 0, 13.33, 7.5, fill_rgb=C_LIGHT)
add_rect(sl, 0, 0, 13.33, 1.1, fill_rgb=C_WHITE)
add_rect(sl, 0, 1.1, 13.33, 0.04, fill_rgb=RGBColor(0xD2,0xD2,0xD7))
add_text(sl, "Flujo de estados — Meta", 0.6, 0.2, 9.0, 0.75, font_size=26, bold=True, color=C_DARK)
add_text(sl, "Ver especificación completa: domain-rules.md §2", 0.6, 0.65, 9.0, 0.35, font_size=11, color=C_MID)

states = [
    (1.0,  3.2, "Pendiente",     C_MID,     "Sistema"),
    (3.5,  3.2, "En Progreso",   C_ACCENT,  "Sistema"),
    (6.5,  2.0, "Completada",    C_SUCCESS, "Monitora / Supervisora"),
    (6.5,  4.4, "Rechazada",     C_DANGER,  "Monitora / Supervisora"),
    (9.5,  3.2, "Scored",        C_WARN,    "Supervisora"),
    (11.7, 3.2, "Aprobada ✓",    C_SUCCESS, "Sistema (auto)"),
]
for (x, y, name, color, actor) in states:
    add_rect(sl, x-0.1, y-0.25, 2.1, 0.9, fill_rgb=color, line_rgb=None)
    add_text(sl, name, x-0.0, y-0.18, 1.9, 0.4, font_size=13, bold=True, color=C_WHITE, align=PP_ALIGN.CENTER)
    add_text(sl, actor, x-0.1, y+0.55, 2.1, 0.35, font_size=9, color=C_MID, align=PP_ALIGN.CENTER)

# Arrows (using thin rectangles as lines — simplified)
arrows = [(3.1, 3.47, 0.4, 0.05), (5.6, 3.47, 0.4, 0.05), (8.5, 2.27, 0.45, 0.05), (8.5, 4.67, 0.45, 0.05), (11.2, 3.47, 0.4, 0.05)]
for (ax, ay, aw, ah) in arrows:
    add_rect(sl, ax, ay, aw, ah, fill_rgb=C_MID)

add_text(sl, "→ Rechazada puede volver a EnProgreso (alumno corrige)", 0.6, 6.5, 12.0, 0.4, font_size=11, color=C_MID)
add_text(sl, "→ Scored → Aprobada es automático si puntaje ≥ mínimo del Pace", 0.6, 6.9, 12.0, 0.4, font_size=11, color=C_MID)

slide_counter(sl, 9)


# ═══════════════════════════════════════════════════════
# SLIDE 10 — STACK TECNOLÓGICO
# ═══════════════════════════════════════════════════════
sl = prs.slides.add_slide(blank_layout)
add_rect(sl, 0, 0, 13.33, 7.5, fill_rgb=C_DARK)
add_rect(sl, 0, 0, 0.06, 7.5, fill_rgb=C_ACCENT)

add_text(sl, "Stack tecnológico", 0.7, 0.3, 10.0, 0.7, font_size=26, bold=True, color=C_WHITE)

layers = [
    ("Frontend",    "Angular 18 + TypeScript",       "SPA tablet-first · CSS propio sin framework UI · HttpClient + interceptores JWT · Guards de rol",       C_SUCCESS),
    ("Backend",     "ASP.NET Core 8 · C#",            "Clean Architecture (Domain / Application / Infrastructure / API) · TDD >90% cobertura · xUnit + Moq", C_ACCENT),
    ("Base de datos","Azure SQL Database",             "Code-First via EF Core · Naming canónico en español · Concurrencia optimista con RowVersion",           C_WARN),
    ("Infraestructura","Azure Web App (Linux)",        "SPA + API como una unidad · Bicep (IaC) · Key Vault · Application Insights · Auto-scaling",             C_PURPLE),
    ("CI/CD",       "GitHub Actions",                 "ng build → dotnet build → test gate → migraciones EF → despliegue zero-downtime",                       C_MID),
]
for i, (layer, tech, detail, color) in enumerate(layers):
    y = 1.2 + i * 1.18
    add_rect(sl, 0.5, y, 12.3, 1.0, fill_rgb=RGBColor(0x2A,0x2A,0x3A), line_rgb=RGBColor(0x44,0x44,0x55), line_pt=0.5)
    add_rect(sl, 0.5, y, 0.07, 1.0, fill_rgb=color)
    add_text(sl, layer, 0.75, y+0.08, 2.0, 0.38, font_size=11, bold=True, color=color)
    add_text(sl, tech,  0.75, y+0.46, 2.5, 0.4, font_size=13, bold=True, color=C_WHITE)
    add_text(sl, detail, 3.5, y+0.2, 9.1, 0.65, font_size=11, color=RGBColor(0xAA,0xAA,0xBB))

slide_counter(sl, 10)


# ═══════════════════════════════════════════════════════
# SLIDE 11 — ARQUITECTURA CLEAN
# ═══════════════════════════════════════════════════════
sl = prs.slides.add_slide(blank_layout)
add_rect(sl, 0, 0, 13.33, 7.5, fill_rgb=C_LIGHT)
add_rect(sl, 0, 0, 13.33, 1.1, fill_rgb=C_WHITE)
add_rect(sl, 0, 1.1, 13.33, 0.04, fill_rgb=RGBColor(0xD2,0xD2,0xD7))
add_text(sl, "Arquitectura — Clean Architecture", 0.6, 0.2, 10.0, 0.75, font_size=26, bold=True, color=C_DARK)

layers_ca = [
    ("API (Presentation)",     "Controllers · Swagger · JWT middleware · Exception handler",   2.0, C_PURPLE),
    ("Infrastructure",         "EF Core repositories · Azure SQL · Key Vault adapter",        3.5, C_WARN),
    ("Application (Use Cases)","Orquestación · IRepository interfaces · IUnitOfWork",         5.0, C_ACCENT),
    ("Domain (Core)",          "Alumno · Pace · AlumnoPace · Meta · Merito · Reglas puras",   6.5, C_SUCCESS),
]
# Draw concentric-style boxes (left to right = outer to inner)
widths = [11.5, 9.5, 7.5, 5.5]
x_starts = [0.9, 1.9, 2.9, 3.9]
colors_bg = [RGBColor(0xF0,0xEE,0xFF), RGBColor(0xFF,0xF3,0xE0), C_WHITE, RGBColor(0xE8,0xF8,0xEE)]
# Just use stacked cards
for i, (name, desc, y, color) in enumerate(layers_ca):
    add_rect(sl, x_starts[i], y-0.1, widths[i], 1.15, fill_rgb=C_WHITE, line_rgb=RGBColor(0xD2,0xD2,0xD7), line_pt=0.5)
    add_rect(sl, x_starts[i], y-0.1, widths[i], 0.07, fill_rgb=color)
    add_text(sl, name, x_starts[i]+0.2, y+0.1, 4.0, 0.4, font_size=13, bold=True, color=color)
    add_text(sl, desc, x_starts[i]+0.2, y+0.52, widths[i]-0.4, 0.4, font_size=11, color=C_MID)

add_text(sl, "⚠  Dependencias solo apuntan hacia adentro: API → App → Domain. Infrastructure → App. Domain no depende de nadie.",
         0.6, 7.1, 12.0, 0.35, font_size=10, color=C_MID)

slide_counter(sl, 11)


# ═══════════════════════════════════════════════════════
# SLIDE 12 — FASES Y CRONOGRAMA
# ═══════════════════════════════════════════════════════
sl = prs.slides.add_slide(blank_layout)
add_rect(sl, 0, 0, 13.33, 7.5, fill_rgb=C_LIGHT)
add_rect(sl, 0, 0, 13.33, 1.1, fill_rgb=C_WHITE)
add_rect(sl, 0, 1.1, 13.33, 0.04, fill_rgb=RGBColor(0xD2,0xD2,0xD7))
add_text(sl, "Fases de desarrollo · ~13–15 semanas", 0.6, 0.2, 10.0, 0.75, font_size=26, bold=True, color=C_DARK)

phases = [
    ("Fase 1", "Cimiento", "2–3 sem", "Repositorio · .NET 8 Clean Arch · Bicep + GitHub Actions · Auth JWT con 3 roles · Staging operativo", C_ACCENT),
    ("Fase 2", "Core",     "3–4 sem", "Entidades TDD (Alumno, Pace, AlumnoPace, Meta) · EF Core + migraciones · Endpoints Alumnos, Paces, Progreso", C_SUCCESS),
    ("Fase 3", "Operación","4–5 sem", "Méritos + privilegios configurables · Angular 18 SPA (Dashboard + Perfil) · Integración frontend–API completa", C_WARN),
    ("Fase 4", "Cierre",   "2–3 sem", "Playwright E2E · Refinamiento UX (Nielsen) · Despliegue producción · Documentación de usuario por rol", C_PURPLE),
]
for i, (num, name, duration, items, color) in enumerate(phases):
    x = 0.4 + i * 3.2
    add_rect(sl, x, 1.4, 3.0, 5.7, fill_rgb=C_WHITE, line_rgb=RGBColor(0xD2,0xD2,0xD7), line_pt=0.5)
    add_rect(sl, x, 1.4, 3.0, 0.6, fill_rgb=color)
    add_text(sl, num,      x+0.15, 1.48, 1.5, 0.3, font_size=10, bold=True, color=C_WHITE)
    add_text(sl, name,     x+0.15, 1.7,  2.7, 0.38, font_size=16, bold=True, color=C_WHITE)
    add_rect(sl, x+0.15, 2.15, 2.7, 0.38, fill_rgb=RGBColor(0xF0,0xF5,0xFF))
    add_text(sl, f"⏱  {duration}", x+0.25, 2.2, 2.5, 0.3, font_size=12, bold=True, color=color)
    # Items
    for j, item in enumerate(items.split(" · ")):
        add_rect(sl, x+0.18, 2.72+j*0.82, 0.05, 0.38, fill_rgb=color)
        add_text(sl, item, x+0.35, 2.67+j*0.82, 2.5, 0.5, font_size=11, color=C_DARK)

add_text(sl, "💡 Núcleo no negociable: TDD · Clean Arch · JWT · CI/CD básico · tablet-first   |   Diferibles: Playwright, App Insights, auto-scaling, Bicep completo",
         0.5, 7.1, 12.5, 0.35, font_size=10, color=C_MID)

slide_counter(sl, 12)


# ═══════════════════════════════════════════════════════
# SLIDE 13 — CIERRE
# ═══════════════════════════════════════════════════════
sl = prs.slides.add_slide(blank_layout)
add_rect(sl, 0, 0, 13.33, 7.5, fill_rgb=C_DARK)
add_rect(sl, 0, 0, 0.06, 7.5, fill_rgb=C_ACCENT)
add_rect(sl, 0, 6.9, 13.33, 0.6, fill_rgb=C_ACCENT)

add_text(sl, "¿Preguntas?", 0.7, 1.0, 11.0, 1.2, font_size=52, bold=True, color=C_WHITE)
add_text(sl, "El spec kit completo está en /spec-kit", 0.7, 2.3, 11.0, 0.5, font_size=18, color=RGBColor(0x99,0x99,0xAA))

docs = ["constitution.md", "domain-rules.md", "backend-api.md", "frontend-ui.md", "infrastructure.md", "phases.md", "tasks.md", "SOW.md"]
for i, doc in enumerate(docs):
    x = 0.7 + (i % 4) * 3.1
    y = 3.1 + (i // 4) * 0.7
    add_rect(sl, x, y, 2.9, 0.5, fill_rgb=RGBColor(0x2A,0x2A,0x3A), line_rgb=RGBColor(0x44,0x44,0x55), line_pt=0.5)
    add_text(sl, f"📄  {doc}", x+0.15, y+0.1, 2.6, 0.32, font_size=11, bold=False, color=RGBColor(0x99,0xCC,0xFF))

add_text(sl, "Talent Institute · Sistema Institucional · Abril 2026", 0.7, 7.0, 11.0, 0.45, font_size=12, bold=True, color=C_WHITE)


# ── SAVE ────────────────────────────────────────────────
out_path = "/Users/dcourtois/Documents/GitHub/talentinstitute-web/spec-kit/TalentInstitute_Presentacion.pptx"
prs.save(out_path)
print(f"OK → {out_path}")
