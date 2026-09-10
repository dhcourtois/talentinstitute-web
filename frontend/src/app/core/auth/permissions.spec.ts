import { MODULOS_POR_ROL, navegacionPara, puedeVer, rolesDe, rutaInicial } from './permissions';

describe('permissions', () => {
  it('Principal es el único con acceso a Staff y Configuración', () => {
    expect(rolesDe('staff')).toEqual(['Principal']);
    expect(rolesDe('configuracion')).toEqual(['Principal']);
  });

  it('Principal y Supervisora no ven el mismo menú (issue #14)', () => {
    const principal = navegacionPara('Principal').map(item => item.modulo);
    const supervisora = navegacionPara('Supervisora').map(item => item.modulo);
    expect(principal).not.toEqual(supervisora);
    expect(principal.length).toBeGreaterThan(supervisora.length);
  });

  it('Supervisora no alcanza los módulos exclusivos de Principal', () => {
    expect(puedeVer('Supervisora', 'staff')).toBeFalse();
    expect(puedeVer('Supervisora', 'configuracion')).toBeFalse();
    expect(puedeVer('Supervisora', 'entrevistas')).toBeTrue();
  });

  it('Monitora no ve los módulos de gestión', () => {
    const modulos = navegacionPara('Monitora').map(item => item.modulo);
    expect(modulos).not.toContain('staff');
    expect(modulos).not.toContain('configuracion');
  });

  it('todo rol tiene un destino inicial válido dentro de su propio menú', () => {
    for (const rol of ['Principal', 'Supervisora', 'Monitora'] as const) {
      const destino = rutaInicial(rol);
      expect(navegacionPara(rol).some(item => item.ruta === destino)).toBeTrue();
    }
  });

  it('sin rol el destino inicial es el login, para no entrar en bucle', () => {
    expect(rutaInicial(null)).toBe('/login');
  });

  it('sin sesión no se muestra ningún módulo', () => {
    expect(navegacionPara(null)).toEqual([]);
    expect(puedeVer(null, 'dashboard')).toBeFalse();
  });

  it('todo rol del personal conserva acceso al perfil del alumno', () => {
    // Acotado al personal al agregarse el rol Padre (issue #8): el padre de
    // familia consulta a sus hijos desde el portal, no desde el módulo de
    // gestión de alumnos, que es una pantalla de operación interna.
    for (const rol of ['Principal', 'Supervisora', 'Monitora'] as const) {
      expect(puedeVer(rol, 'alumnos')).toBeTrue();
    }
  });

  // ── Portal de padres de familia (issue #8) ───────────────────────────────

  it('el Padre solo alcanza el portal', () => {
    expect(MODULOS_POR_ROL.Padre).toEqual(['portal']);
    expect(rutaInicial('Padre')).toBe('/portal');
  });

  it('el Padre no alcanza ningún módulo del personal', () => {
    const delPersonal = ['dashboard', 'alumnos', 'paces', 'entrevistas', 'staff', 'configuracion', 'padres'] as const;
    for (const modulo of delPersonal) {
      expect(puedeVer('Padre', modulo)).toBeFalse();
    }
  });

  it('ningún rol del personal alcanza el portal del padre', () => {
    for (const rol of ['Principal', 'Supervisora', 'Monitora'] as const) {
      expect(puedeVer(rol, 'portal')).toBeFalse();
    }
  });

  it('solo el Principal administra las cuentas de padres', () => {
    expect(rolesDe('padres')).toEqual(['Principal']);
  });

  it('el menú del Padre no comparte una sola entrada con el del personal', () => {
    const delPadre = navegacionPara('Padre').map(item => item.modulo);
    for (const rol of ['Principal', 'Supervisora', 'Monitora'] as const) {
      const delPersonal = navegacionPara(rol).map(item => item.modulo);
      expect(delPadre.some(modulo => delPersonal.includes(modulo))).toBeFalse();
    }
  });

  it('todo rol, incluido el Padre, tiene un destino inicial dentro de su propio menú', () => {
    for (const rol of ['Principal', 'Supervisora', 'Monitora', 'Padre'] as const) {
      const destino = rutaInicial(rol);
      const suyas = navegacionPara(rol).map(item => item.ruta);
      expect(suyas).toContain(destino);
    }
  });
});
