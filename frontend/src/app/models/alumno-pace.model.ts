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
  numeroPace?: string;
  puntajeMaximo?: number;
  pace?: import('./pace.model').Pace;
  fechaInicio: string;
  fechaCompletado?: string;
  puntajeFinal?: number;
  estado: EstadoAlumnoPace;
  /** Siguiente paso del flujo ACE, o ausente si el PACE ya cerró. */
  siguienteAccion?: AccionPace | null;
  /** Verdadero cuando el PACE ya está Completado o Fallido. */
  cerrado?: boolean;
}

export type AccionPace =
  | 'MarcarListoParaAutoTest'
  | 'RegistrarAutoTest'
  | 'ProgramarTestFinal'
  | 'EvaluarTestFinal';

export interface AvanzarEstadoPaceRequest {
  accion: AccionPace;
  /** Requerido al registrar el auto-test y al evaluar el test final. */
  exitoso?: boolean;
  puntajeFinal?: number;
}
