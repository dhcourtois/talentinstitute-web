export type TipoMerito = 'Merito' | 'Demerito';

export interface Merito {
  id: string;
  alumnoId: string;
  staffId: string;
  staffName?: string;
  tipo: TipoMerito;
  puntos: number;
  motivo: string;
  fechaAplicado: string;
  revocado: boolean;
  staffIdRevoco?: string;
  staffRevocoName?: string;
  fechaRevocacion?: string;
}

export interface RegistrarMeritoRequest {
  alumnoId: string;
  tipo: TipoMerito;
  puntos: number;
  motivo: string;
}
