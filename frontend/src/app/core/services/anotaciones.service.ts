import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Anotacion, RegistrarAnotacionRequest } from '../../models';

@Injectable({ providedIn: 'root' })
export class AnotacionesService {
  private readonly url = `${environment.apiUrl}/Anotaciones`;

  constructor(private http: HttpClient) {}

  getByAlumno(alumnoId: string): Observable<Anotacion[]> {
    return this.http.get<Anotacion[]>(`${this.url}/alumno/${alumnoId}`);
  }

  registrar(request: RegistrarAnotacionRequest): Observable<{ id: string; message: string }> {
    return this.http.post<{ id: string; message: string }>(this.url, request);
  }
}
