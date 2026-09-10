export type PrivilegioFlag = 'Oficina' | 'Comedor' | 'Patio' | 'Biblioteca' | 'Actividades';

export interface Alumno {
  id: string;
  numeroMatricula: string;
  nombre: string;
  apellido: string;
  nivel: string;
  fechaIngreso?: string;
  privilegeStatus?: PrivilegeStatus;
  privilegiosManuales?: PrivilegiosManuales;
  privilegioStatus?: number;
  privilegiosActivos?: PrivilegioFlag[];
  balanceMeritos?: number;
}

export interface PrivilegeStatus {
  oficina: boolean;
  comedor: boolean;
  patio: boolean;
  biblioteca: boolean;
  actividades: boolean;
}

/**
 * Excepciones manuales por privilegio (issue #21).
 * `null` o ausente significa que ese privilegio sigue decidido por el balance.
 */
export interface PrivilegiosManuales {
  oficina: boolean | null;
  comedor: boolean | null;
  patio: boolean | null;
  biblioteca: boolean | null;
  actividades: boolean | null;
}

/** Lo que la interfaz puede pedir para un privilegio. */
export type ModoPrivilegio = 'auto' | 'activo' | 'inactivo';
