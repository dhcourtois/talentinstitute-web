import { Rol } from './auth.model';

export interface Staff {
  id: string;
  email: string;
  rol: Rol;
  activo: boolean;
}
