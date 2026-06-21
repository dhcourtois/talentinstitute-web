export type PrivilegioFlag = 'Oficina' | 'Comedor' | 'Patio' | 'Biblioteca' | 'Actividades';

export interface Alumno {
  id: string;
  numeroMatricula: string;
  nombre: string;
  apellido: string;
  nivel: string;
  fechaIngreso?: string;
  privilegeStatus?: PrivilegeStatus;
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
