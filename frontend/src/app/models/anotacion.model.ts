/** Observación semanal sobre un alumno (issue #20). */
export interface Anotacion {
  id: string;
  alumnoId: string;
  staffId: string;
  staffName?: string;
  /** Lunes de la semana a la que pertenece, en formato `yyyy-MM-dd`. */
  semanaInicio: string;
  texto: string;
  fechaCreacion: string;
}

export interface RegistrarAnotacionRequest {
  alumnoId: string;
  texto: string;
  /** Día al que corresponde. Omitirlo la registra en la semana en curso. */
  fecha?: string;
}
