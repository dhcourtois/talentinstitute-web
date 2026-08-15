import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Merito, RegistrarMeritoRequest } from '../../models';

@Injectable({ providedIn: 'root' })
export class MeritosService {
  private readonly url = `${environment.apiUrl}/Meritos`;

  constructor(private http: HttpClient) {}

  getByAlumno(alumnoId: string): Observable<Merito[]> {
    return this.http.get<Merito[]>(`${this.url}/alumno/${alumnoId}`);
  }

  registrar(request: RegistrarMeritoRequest): Observable<Merito> {
    return this.http.post<Merito>(this.url, request);
  }

  revocar(meritoId: string): Observable<Merito> {
    return this.http.patch<Merito>(`${this.url}/${meritoId}/revocar`, {});
  }
}
