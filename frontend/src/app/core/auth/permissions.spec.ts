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

  it('todo rol conserva acceso al perfil del alumno', () => {
    for (const rol of Object.keys(MODULOS_POR_ROL) as Array<keyof typeof MODULOS_POR_ROL>) {
      expect(puedeVer(rol, 'alumnos')).toBeTrue();
    }
  });
});
