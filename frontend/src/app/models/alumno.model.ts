export type PrivilegioFlag = 'Oficina' | 'Comedor' | 'Patio' | 'Biblioteca' | 'Actividades';

export interface Alumno {
  id: string;
  numeroMatricula: string;
  nombre: string;
  apellido: string;
  nivel: string;
  privilegioStatus: number;
  privilegiosActivos?: PrivilegioFlag[];
  balanceMeritos?: number;
}
