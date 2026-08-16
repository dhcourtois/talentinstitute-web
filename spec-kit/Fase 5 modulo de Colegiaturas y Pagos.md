# Especificación de Requerimientos: Fase 5 - Módulo de Colegiaturas y Pagos
**Institución:** Talent Institute  
**Documento:** Requerimientos Funcionales y Técnicos de Software  
**Fase del Proyecto:** Fase 5 — Módulo de Colegiaturas, Pagos y Emisión de Recibos  
**Estado:** Propuesta Técnica y Arquitectura Funcional  

---

## 1. Resumen Ejecutivo y Objetivo

El presente documento formaliza los requerimientos de negocio, flujos operativos, reglas de cálculo financiero y arquitectura técnica para la **Fase 5: Módulo de Colegiaturas y Pagos** del sistema de gestión escolar de **Talent Institute**.

El objetivo principal es reemplazar los procesos manuales y hojas de cálculo (Excel/Word) por un módulo integral, automatizado y seguro que permita:
1. Registrar y consultar el estado de cuenta por alumno y núcleo familiar en tiempo real.
2. Aplicar automáticamente políticas de descuentos (hermanos, pronto pago en efectivo) y penalizaciones por mora.
3. Controlar pagos parciales, proyecciones anuales y compras de material didáctico (PACEs).
4. Generar automáticamente **Recibos de Donación** digitales en formato PDF con diseño institucional y numeración consecutiva.
5. Permitir la distribución ágil de comprobantes vía correo electrónico y mensajería WhatsApp.

---

## 2. Estructura de Conceptos de Cobro (Donaciones y Cuotas)

El sistema debe gestionar los siguientes conceptos configurables por nivel educativo y ciclo escolar:

| Concepto | Tipo / Frecuencia | Monto Referencial | Descripción / Reglas |
| :--- | :--- | :--- | :--- |
| **Donación Mensual (Colegiatura)** | Recurrente (Mensual, Sep - Jun) | • Kindergarten: $1,800.00 MXN<br>• Elementary: $2,950.00 – $3,560.00 MXN | Colegiatura del mes en curso correspondiente al nivel del alumno. |
| **Acceso Anual / Sistema** | Único Anual | $1,980.00 – $2,200.00 MXN | Cuota de inscripción / acceso a plataforma educativa anual. |
| **Tienda de Recompensas** | Único Anual / Por Evento | $1,000.00 MXN | Fondo de incentivos y premiación de alumnos. |
| **Materiales Didácticos (PACEs)** | Anual / Fraccionado | Proyección ~72 unidades (~$6,015.24 MXN) | Cuota de cuadernillos/material PACEs. Permite división en 2 exhibiciones (ej. 15 Julio y 15 Diciembre) o abonos parciales. |
| **Penalización por Mora** | Cargo Adicional | $200.00 MXN | Recargo fijo aplicado a mensualidades pagadas fuera del periodo ordinario. |

---

## 3. Reglas de Negocio y Lógica Financiera

### 3.1. Fechas Límite y Periodo Ordinario
* **Periodo Regular de Pago:** Del día **1 al 15** de cada mes.
* **Periodo Extemporáneo:** A partir del día **16** del mes en curso hasta fin de mes.
* **Aplicación de Recargo:** Todo pago de colegiatura mensual recibido del día 16 en adelante genera un cargo adicional automático de **$200.00 MXN**.

### 3.2. Descuentos por Alumno Único (Pronto Pago / Efectivo)
* Si un alumno no cuenta con descuento familiar (es hijo único inscrito en la institución) y realiza su pago en **efectivo dentro del periodo regular (1 al 15 de cada mes)**, es elegible para un **10% de descuento** sobre la Donación Mensual en los niveles aplicables (ej. Elementary).

### 3.3. Descuentos por Hermanos (Vínculo Familiar Multifamiliar)
* Cuando una familia inscribe a **2 o más hermanos**:
  * El hijo de mayor colegiatura/grado paga cuota completa.
  * El segundo hijo (o el de menor cuota mensual) recibe un **descuento porcentual (20% al 30%)** sobre su donación mensual base.
  * Los conceptos adicionales (Acceso Anual, Tienda de Recompensas y PACEs) se cobran sin descuento, salvo autorización administrativa expresa.

### 3.4. Pagos Parciales y Saldos a Favor / Pendientes
* El sistema debe permitir registrar abonos a cuenta (`A/C`), por ejemplo en el rubro de PACEs o Inscripción:
  $$\text{Saldo Restante} = \text{Monto Total Proyectado} - \sum \text{Abonos Registrados}$$
* Los estados de cada concepto se clasificarán automáticamente como:
  * `PAGADO`: Monto total liquidado.
  * `PARCIAL`: Abono registrado con saldo pendiente.
  * `PENDIENTE`: Sin abonos registrados al momento.
  * `VENCIDO`: Sin liquidar después de la fecha límite estipulada.

---

## 4. Módulo de Emisión y Distribución de Recibos Digitales

### 4.1. Generación de Recibos en PDF
El sistema debe contar con un motor de renderizado de comprobantes con las siguientes características:
* **Encabezado Institucional:** Logotipo oficial de *Talent Institute*, datos fiscales/asociación y folio consecutivo oficial (ej. `No. Recibo 001`, `002`).
* **Datos del Alumno:** Nombre completo, nivel educativo (Kindergarten, Elementary, etc.), ciclo escolar.
* **Detalle del Concepto:** Desglose del pago realizado (ej. *Donación mensual de $2,950.00 del mes de Septiembre* o *Donación Anual Acceso/Sistema*).
* **Fecha y Método:** Fecha de emisión y método de pago (Efectivo, Transferencia, Depósito).
* **Marca de Agua / Estatus:** Banderas visuales para pagos completos o recibos informativos con leyenda `(pendiente de pago)` en caso de adeudo.

### 4.2. Canales de Distribución
1. **Envío por WhatsApp:** Generación de mensaje predeterminado con enlace directo o adjunto del PDF hacia el teléfono celular registrado del tutor.
2. **Envío por Correo Electrónico:** Envío automático del PDF adjunto a la dirección de correo registrada de la familia.
3. **Descarga Directa:** Opción para descargar o imprimir el recibo inmediatamente tras capturar el pago.

---

## 5. Arquitectura del Modelo de Datos (Esquema Relacional Sugerido)

```sql
-- Tabla de Familias / Tutores
CREATE TABLE familias (
    id_familia SERIAL PRIMARY KEY,
    nombre_tutor VARCHAR(150) NOT NULL,
    telefono_contacto VARCHAR(20) NOT NULL,
    email_tutor VARCHAR(120),
    fecha_registro TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Tabla de Alumnos
CREATE TABLE alumnos (
    id_alumno SERIAL PRIMARY KEY,
    id_familia INT REFERENCES familias(id_familia),
    nombre_completo VARCHAR(150) NOT NULL,
    nivel_educativo VARCHAR(50) NOT NULL, -- 'Kindergarten', 'Elementary'
    activo BOOLEAN DEFAULT TRUE
);

-- Tabla de Conceptos de Cobro / Catálogo
CREATE TABLE conceptos_cobro (
    id_concepto SERIAL PRIMARY KEY,
    nombre_concepto VARCHAR(100) NOT NULL,
    monto_base NUMERIC(10,2) NOT NULL,
    es_recurrente BOOLEAN DEFAULT FALSE
);

-- Tabla de Cargos y Cuentas por Cobrar
CREATE TABLE cargos_alumno (
    id_cargo SERIAL PRIMARY KEY,
    id_alumno INT REFERENCES alumnos(id_alumno),
    id_concepto INT REFERENCES conceptos_cobro(id_concepto),
    ciclo_escolar VARCHAR(20) NOT NULL,
    periodo VARCHAR(30), -- 'Septiembre 2026', 'Anual 2026-2027'
    monto_original NUMERIC(10,2) NOT NULL,
    descuento_aplicado NUMERIC(10,2) DEFAULT 0.00,
    recargo_mora NUMERIC(10,2) DEFAULT 0.00,
    monto_total_pagar NUMERIC(10,2) NOT NULL,
    fecha_limite DATE NOT NULL,
    estado VARCHAR(20) DEFAULT 'PENDIENTE' -- 'PENDIENTE', 'PARCIAL', 'PAGADO', 'VENCIDO'
);

-- Tabla de Pagos y Transacciones
CREATE TABLE pagos (
    id_pago SERIAL PRIMARY KEY,
    id_cargo INT REFERENCES cargos_alumno(id_cargo),
    folio_recibo VARCHAR(50) UNIQUE NOT NULL,
    monto_pagado NUMERIC(10,2) NOT NULL,
    fecha_pago DATE NOT NULL,
    metodo_pago VARCHAR(50) NOT NULL, -- 'EFECTIVO', 'TRANSFERENCIA'
    comprobante_url VARCHAR(255)
);
```

---

## 6. Flujo de Trabajo Operativo (Workflow)

```
[Inicio de Mes]
       │
       ▼
[Generación de Cargos Mensuales por Nivel]
       │
       ▼
[¿Familia tiene 2+ alumnos?] ─── Sí ───► [Aplicar 20%-30% Descuento en colegiatura menor]
       │ No
       ▼
[Recepción de Pago]
       │
       ├─► ¿Pago en Efectivo (1-15 del mes)? ──► [Aplicar 10% Descuento Pronto Pago]
       │
       ├─► ¿Pago extemporáneo (> día 15)? ────► [Sumar Penalización $200.00 MXN]
       │
       ▼
[Registro de Transacción / Abono en el Sistema]
       │
       ▼
[Actualización de Estado de Cuenta (Pagado / Saldo Pendiente)]
       │
       ▼
[Generación Automática de Recibo Digital PDF con Folio Único]
       │
       ▼
[Dispersión: Envío por WhatsApp / Correo Electrónico al Tutor]
```

---

## 7. Próximos Pasos para la Implementación
1. **Aprobación de la Especificación:** Validación de montos, porcentajes y políticas de descuento con la dirección escolar.
2. **Desarrollo Backend:** Creación de endpoints REST para registro de cobros, cálculo de descuentos y motor de plantillas PDF.
3. **Desarrollo Frontend:** Interfaz de caja/cobranza ágil con vista resumida de estatus por familia y botón de envío de recibos.
4. **Pruebas Integrales:** Simulación con los casos testigo (Alumnos regulares, hermanos en distintos grados, pagos parciales de PACEs).
