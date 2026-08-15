import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Meta, CrearMetaRequest, ActualizarEstadoMetaRequest } from '../../models';

@Injectable({ providedIn: 'root' })
export class ProgresoService {
  private readonly url = `${environment.apiUrl}/Progreso`;

  constructor(private http: HttpClient) {}

  getSemana(alumnoId: string, fechaInicio: string): Observable<Meta[]> {
    return this.http.get<Meta[]>(`${this.url}/${alumnoId}/semana/${fechaInicio}`);
  }

  crearMeta(request: CrearMetaRequest): Observable<Meta> {
    return this.http.post<Meta>(this.url, request);
  }

  actualizarEstadoMeta(metaId: string, request: ActualizarEstadoMetaRequest): Observable<Meta> {
    return this.http.put<Meta>(`${this.url}/metas/${metaId}/estatus`, request);
  }
}
