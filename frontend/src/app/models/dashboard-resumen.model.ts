export interface DashboardResumen {
  totalAlumnosActivos: number;
  metasHoy: number;
  pacesEnRevision: number;
  totalAlertas: number;
  metasPorDia: MetasDia[];
}

export interface MetasDia {
  fecha: string;
  completadas: number;
  total: number;
}
