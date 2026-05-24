import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Alumno } from '../../models';

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

  create(alumno: Partial<Alumno>): Observable<Alumno> {
    return this.http.post<Alumno>(this.url, alumno);
  }

  update(id: string, alumno: Partial<Alumno>): Observable<Alumno> {
    return this.http.put<Alumno>(`${this.url}/${id}`, alumno);
  }
}
