export type Rol = 'Principal' | 'Supervisora' | 'Monitora';

export interface Staff {
  id: string;
  email: string;
  rol: Rol;
  activo: boolean;
}
