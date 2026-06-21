export type TipoAlerta = 'SinMeta' | 'PaceListoAutoTest' | 'PaceVencido';

export interface Alerta {
  id?: string;
  alumnoId: string;
  nombreAlumno?: string;
  nombre?: string;
  apellido?: string;
  numeroMatricula?: string;
  nivel?: string;
  ultimaMetaFecha?: string;
  tipo?: TipoAlerta;
  mensaje?: string;
  fechaGenerada?: string;
}
