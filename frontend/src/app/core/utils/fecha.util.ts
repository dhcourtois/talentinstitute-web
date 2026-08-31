/**
 * Formatea una fecha como `yyyy-MM-dd` usando los campos locales.
 *
 * `Date.toISOString()` convierte a UTC y, en husos detrás de Greenwich, adelanta
 * la fecha a partir de cierta hora de la tarde. La API espera el día tal como lo
 * ve el usuario, así que nunca debe formatearse una fecha de calendario en UTC.
 */
export function toLocalIsoDate(date: Date): string {
  const year = date.getFullYear();
  const month = `${date.getMonth() + 1}`.padStart(2, '0');
  const day = `${date.getDate()}`.padStart(2, '0');
  return `${year}-${month}-${day}`;
}

/** Fecha de hoy en formato `yyyy-MM-dd`. */
export function todayIso(reference: Date = new Date()): string {
  return toLocalIsoDate(reference);
}

/**
 * Lunes de la semana en curso en formato `yyyy-MM-dd`.
 * El endpoint de metas semanales solo acepta lunes.
 */
export function weekStartIso(reference: Date = new Date()): string {
  const date = new Date(reference.getTime());
  const day = date.getDay();
  const diff = day === 0 ? -6 : 1 - day;
  date.setDate(date.getDate() + diff);
  return toLocalIsoDate(date);
}
