export type EstadoMeta =
  | 'Pendiente'
  | 'EnProgreso'
  | 'Completada'
  | 'Rechazada'
  | 'Scored'
  | 'Aprobada';

export type Turno = 'Mañana' | 'Tarde';

export interface Meta {
  id: string;
  alumnoPaceId: string;
  turno: Turno;
  paginasObjetivo: number;
  fechaObjetivo: string;
  puntajeObtenido?: number;
  estado: EstadoMeta;
}

export interface ActualizarEstadoMetaRequest {
  estado: EstadoMeta;
  puntajeObtenido?: number;
}

export interface CrearMetaRequest {
  alumnoId: string;
  alumnoPaceId: string;
  turno: Turno;
  paginasObjetivo: number;
  fechaObjetivo: string;
}
