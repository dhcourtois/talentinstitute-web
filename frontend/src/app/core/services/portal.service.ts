import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Hijo, Merito, Meta } from '../../models';

/**
 * Consultas del portal de padres (issue #8).
 *
 * Ningún método recibe el id del padre: el backend lo toma del token y filtra
 * con él. Mandarlo desde aquí lo volvería un dato que el cliente puede cambiar.
 */
@Injectable({ providedIn: 'root' })
export class PortalService {
  private readonly url = `${environment.apiUrl}/Portal`;

  constructor(private http: HttpClient) {}

  getHijos(): Observable<Hijo[]> {
    return this.http.get<Hijo[]>(`${this.url}/hijos`);
  }

  getMetas(alumnoId: string, semana?: string): Observable<Meta[]> {
    let params = new HttpParams();
    if (semana) params = params.set('semana', semana);
    return this.http.get<Meta[]>(`${this.url}/hijos/${alumnoId}/metas`, { params });
  }

  getMeritos(alumnoId: string): Observable<Merito[]> {
    return this.http.get<Merito[]>(`${this.url}/hijos/${alumnoId}/meritos`);
  }
}
