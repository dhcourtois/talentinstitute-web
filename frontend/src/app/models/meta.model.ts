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
  paginaInicial: number;
  paginaFinal: number;
  /** Derivada del rango; la envía el backend ya calculada. */
  paginasObjetivo: number;
  fechaObjetivo: string;
  puntajeObtenido?: number;
  estado: EstadoMeta;
  materia?: string;
  numeroPace?: number;
}

export interface ActualizarEstadoMetaRequest {
  estado: EstadoMeta;
  puntajeObtenido?: number;
}

export interface CrearMetaRequest {
  alumnoId: string;
  alumnoPaceId: string;
  turno: Turno;
  paginaInicial: number;
  /** Inclusive. Igual a la inicial registra una sola página. */
  paginaFinal: number;
  fechaObjetivo: string;
}
