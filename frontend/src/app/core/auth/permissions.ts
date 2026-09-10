import { Rol } from '../../models';

/** Módulos navegables del sistema. */
export type Modulo =
  | 'dashboard'
  | 'alumnos'
  | 'paces'
  | 'entrevistas'
  | 'staff'
  | 'configuracion'
  | 'padres'
  | 'portal';

/**
 * Matriz de permisos por rol — fuente única de verdad del frontend.
 *
 * Debe mantenerse en correspondencia con los atributos [Authorize(Roles=…)]
 * de los controladores: esta matriz decide qué se muestra, el backend decide
 * qué se permite. Cambiar una sin la otra deja una de las dos mintiendo.
 *
 * Acordado con el cliente (issue #14):
 *   Principal   → acceso completo.
 *   Supervisora → todo excepto Staff y Configuración.
 *   Monitora    → operación diaria sobre el alumno.
 *   Padre       → solo el portal de consulta (issue #8).
 */
export const MODULOS_POR_ROL: Record<Rol, readonly Modulo[]> = {
  Principal: ['dashboard', 'alumnos', 'paces', 'entrevistas', 'staff', 'configuracion', 'padres'],
  Supervisora: ['dashboard', 'alumnos', 'paces', 'entrevistas'],
  // La Monitora entra al dashboard porque es su lista de alumnos; las métricas
  // ejecutivas ya se ocultan dentro de la pantalla.
  Monitora: ['dashboard', 'alumnos'],
  // El padre de familia no comparte ni un módulo con el personal. La lista de
  // un solo elemento es deliberada: cualquier módulo que se agregue aquí le
  // abriría una pantalla de operación.
  Padre: ['portal']
};

export interface ItemNavegacion {
  modulo: Modulo;
  etiqueta: string;
  ruta: string;
  /** Falso mientras la pantalla del módulo no exista, para no publicar enlaces muertos. */
  disponible: boolean;
}

/** Orden de aparición en el menú lateral. */
export const NAVEGACION: readonly ItemNavegacion[] = [
  { modulo: 'dashboard', etiqueta: 'Dashboard', ruta: '/dashboard', disponible: true },
  { modulo: 'alumnos', etiqueta: 'Alumnos', ruta: '/alumnos', disponible: true },
  { modulo: 'paces', etiqueta: 'PACEs', ruta: '/paces', disponible: true },
  { modulo: 'entrevistas', etiqueta: 'Entrevistas', ruta: '/entrevistas', disponible: true },
  { modulo: 'staff', etiqueta: 'Staff', ruta: '/staff', disponible: true },
  { modulo: 'configuracion', etiqueta: 'Configuración', ruta: '/configuracion', disponible: true },
  { modulo: 'padres', etiqueta: 'Padres de familia', ruta: '/padres', disponible: true },
  { modulo: 'portal', etiqueta: 'Mis hijos', ruta: '/portal', disponible: true }
];

export function puedeVer(rol: Rol | null, modulo: Modulo): boolean {
  return rol ? MODULOS_POR_ROL[rol].includes(modulo) : false;
}

export function navegacionPara(rol: Rol | null): ItemNavegacion[] {
  return NAVEGACION.filter(item => item.disponible && puedeVer(rol, item.modulo));
}

/**
 * Primer destino válido para un rol. El guard lo usa como caída: mandar a
 * `/dashboard` sin comprobar el permiso genera un bucle de redirección para
 * quien no puede verlo.
 */
export function rutaInicial(rol: Rol | null): string {
  return navegacionPara(rol)[0]?.ruta ?? '/login';
}

/** Rol que puede entrar a un módulo, para alimentar `data.roles` de las rutas. */
export function rolesDe(modulo: Modulo): Rol[] {
  return (Object.keys(MODULOS_POR_ROL) as Rol[]).filter(rol => puedeVer(rol, modulo));
}
