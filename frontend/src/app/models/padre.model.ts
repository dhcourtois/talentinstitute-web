/** Alumno visto desde el portal del padre (issue #8). */
export interface Hijo {
  id: string;
  numeroMatricula: string;
  nombre: string;
  apellido: string;
  nivel: string;
  balanceMeritos: number;
  privilegiosActivos: string[];
}

/** Cuenta de padre de familia, en la pantalla de administración del Principal. */
export interface PadreFamilia {
  id: string;
  email: string;
  nombre: string;
  activo: boolean;
  fechaCreacion: string;
  hijos: HijoVinculado[];
}

export interface HijoVinculado {
  id: string;
  numeroMatricula: string;
  nombreCompleto: string;
}

export interface CrearPadreFamiliaRequest {
  email: string;
  password: string;
  nombre: string;
}
