import { toLocalIsoDate, todayIso, weekStartIso } from './fecha.util';

describe('fecha.util', () => {
  it('formatea la fecha con los campos locales, no en UTC', () => {
    // 23:30 local: toISOString() daría el día siguiente en husos negativos.
    const nocheDelDia = new Date(2026, 7, 31, 23, 30, 0);
    expect(toLocalIsoDate(nocheDelDia)).toBe('2026-08-31');
  });

  it('rellena mes y día con ceros', () => {
    expect(toLocalIsoDate(new Date(2026, 0, 5, 12, 0, 0))).toBe('2026-01-05');
  });

  it('devuelve el lunes de la semana en curso', () => {
    // Miércoles 2 de septiembre de 2026 → lunes 31 de agosto.
    expect(weekStartIso(new Date(2026, 8, 2, 10, 0, 0))).toBe('2026-08-31');
  });

  it('devuelve el mismo lunes cuando la referencia ya es lunes por la noche', () => {
    // Este es el caso que rompía el perfil del alumno cada tarde.
    expect(weekStartIso(new Date(2026, 7, 31, 20, 0, 0))).toBe('2026-08-31');
  });

  it('trata el domingo como cierre de la semana en curso', () => {
    // Domingo 6 de septiembre de 2026 → lunes 31 de agosto.
    expect(weekStartIso(new Date(2026, 8, 6, 9, 0, 0))).toBe('2026-08-31');
  });

  it('siempre devuelve un lunes', () => {
    for (let offset = 0; offset < 14; offset++) {
      const referencia = new Date(2026, 7, 24 + offset, 21, 0, 0);
      const [year, month, day] = weekStartIso(referencia).split('-').map(Number);
      expect(new Date(year, month - 1, day).getDay()).toBe(1);
    }
  });

  it('todayIso coincide con la fecha local de la referencia', () => {
    expect(todayIso(new Date(2026, 11, 31, 22, 45, 0))).toBe('2026-12-31');
  });
});
