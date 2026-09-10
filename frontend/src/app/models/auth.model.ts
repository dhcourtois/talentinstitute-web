/**
 * `Padre` no es personal del colegio: identifica una cuenta del portal de
 * consulta (issue #8). Se incluye aquí porque es lo que viaja en el token y
 * lo que la matriz de permisos tiene que saber resolver.
 */
export type Rol = 'Principal' | 'Supervisora' | 'Monitora' | 'Padre';

export interface LoginRequest {
  email: string;
  password: string;
  vistaInicial: string;
}

export interface LoginResponse {
  token: string;
}

export interface TokenPayload {
  sub?: string;
  email?: string;
  rol?: string;
  role?: string;
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'?: string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'?: string;
  exp: number;
}
