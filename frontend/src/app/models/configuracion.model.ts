export interface ConfiguracionPrivilegios {
  id?: string;
  fechaActualizacion?: string;
  staffIdActualizo?: string | null;
  umbralOficina: number;
  umbralOficinaRevocado: number;
  umbralComedor: number;
  umbralComedorRevocado: number;
  umbralPatio: number;
  umbralPatioRevocado: number;
  umbralBiblioteca: number;
  umbralBibliotecaRevocado: number;
  umbralActividades: number;
  umbralActividadesRevocado: number;
}

export type ActualizarConfiguracionRequest = Omit<
  ConfiguracionPrivilegios,
  'id' | 'fechaActualizacion' | 'staffIdActualizo'
>;
