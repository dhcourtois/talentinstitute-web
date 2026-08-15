export type EstadoAlumnoPace =
  | 'Asignado'
  | 'EnProgreso'
  | 'ListoParaAutoTest'
  | 'AutoTestOk'
  | 'AutoTestFallido'
  | 'EnTestFinal'
  | 'Completado'
  | 'Fallido';

export interface AlumnoPace {
  id: string;
  alumnoId: string;
  paceId: string;
  materia?: string;
  numeroPace?: number;
  puntajeMaximo?: number;
  pace?: import('./pace.model').Pace;
  fechaInicio: string;
  fechaCompletado?: string;
  puntajeFinal?: number;
  estado: EstadoAlumnoPace;
}
