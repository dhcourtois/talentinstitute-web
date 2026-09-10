export interface Pace {
  id: string;
  materia: string;
  /** Alfanumérico: 1045 o RR01. Normalizado a mayúsculas por el backend. */
  numeroPace: string;
  puntajeMaximo: number;
  puntajeMinimoAprobacion?: number;
  /** Total de páginas del cuadernillo. Nulo en los PACEs capturados antes del issue #6. */
  totalPaginas?: number | null;
}
