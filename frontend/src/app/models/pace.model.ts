export interface Pace {
  id: string;
  materia: string;
  numeroPace: number;
  puntajeMaximo: number;
  puntajeMinimoAprobacion?: number;
  /** Total de páginas del cuadernillo. Nulo en los PACEs capturados antes del issue #6. */
  totalPaginas?: number | null;
}
