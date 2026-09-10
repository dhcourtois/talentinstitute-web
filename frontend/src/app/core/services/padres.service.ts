import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CrearPadreFamiliaRequest, PadreFamilia } from '../../models';

/** Administración de cuentas de padres. Solo el Principal (issue #8). */
@Injectable({ providedIn: 'root' })
export class PadresService {
  private readonly url = `${environment.apiUrl}/PadresFamilia`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<PadreFamilia[]> {
    return this.http.get<PadreFamilia[]>(this.url);
  }

  crear(request: CrearPadreFamiliaRequest): Observable<{ id: string; message: string }> {
    return this.http.post<{ id: string; message: string }>(this.url, request);
  }

  vincularHijo(padreId: string, alumnoId: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.url}/${padreId}/hijos`, { alumnoId });
  }

  desvincularHijo(padreId: string, alumnoId: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.url}/${padreId}/hijos/${alumnoId}`);
  }

  cambiarEstado(padreId: string, activo: boolean): Observable<{ message: string }> {
    return this.http.patch<{ message: string }>(`${this.url}/${padreId}/estado`, { activo });
  }
}
