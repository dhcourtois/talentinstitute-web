export type TipoAlerta = 'SinMeta' | 'PaceListoAutoTest' | 'PaceVencido';

export interface Alerta {
  id: string;
  alumnoId: string;
  nombreAlumno: string;
  tipo: TipoAlerta;
  mensaje: string;
  fechaGenerada: string;
}
