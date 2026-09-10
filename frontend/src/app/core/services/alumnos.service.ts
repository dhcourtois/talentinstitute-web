import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Alumno, PrivilegioFlag } from '../../models';

@Injectable({ providedIn: 'root' })
export class AlumnosService {
  private readonly url = `${environment.apiUrl}/Alumnos`;

  constructor(private http: HttpClient) {}

  getAll(nivel?: string, page = 1, pageSize = 20): Observable<Alumno[]> {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);
    if (nivel) params = params.set('nivel', nivel);
    return this.http.get<Alumno[]>(this.url, { params });
  }

  getById(id: string): Observable<Alumno> {
    return this.http.get<Alumno>(`${this.url}/${id}`);
  }

  create(
    alumno: Pick<Alumno, 'numeroMatricula' | 'nombre' | 'apellido' | 'nivel'> & { fechaIngreso?: string }
  ): Observable<{ id: string; message: string }> {
    return this.http.post<{ id: string; message: string }>(this.url, alumno);
  }

  update(
    id: string,
    alumno: Pick<Alumno, 'nombre' | 'apellido' | 'nivel'> & { fechaIngreso?: string }
  ): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.url}/${id}`, alumno);
  }

  /**
   * Fuerza un privilegio a activo o inactivo (issue #21).
   * `activo: null` lo devuelve a automático, donde manda el balance de méritos.
   */
  actualizarPrivilegio(
    alumnoId: string,
    privilegio: PrivilegioFlag,
    activo: boolean | null
  ): Observable<{ message: string }> {
    return this.http.patch<{ message: string }>(`${this.url}/${alumnoId}/privilegios`, { privilegio, activo });
  }
}
