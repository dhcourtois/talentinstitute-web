import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Pace, AlumnoPace } from '../../models';

@Injectable({ providedIn: 'root' })
export class PacesService {
  private readonly url = `${environment.apiUrl}/Paces`;

  constructor(private http: HttpClient) {}

  getCatalogo(materia?: string): Observable<Pace[]> {
    let params = new HttpParams();
    if (materia) params = params.set('subject', materia);
    return this.http.get<Pace[]>(this.url, { params });
  }

  getByAlumno(alumnoId: string): Observable<AlumnoPace[]> {
    return this.http.get<AlumnoPace[]>(`${this.url}/alumno/${alumnoId}`);
  }

  create(pace: Partial<Pace>): Observable<Pace> {
    return this.http.post<Pace>(this.url, pace);
  }

  asignar(alumnoId: string, paceId: string): Observable<AlumnoPace> {
    return this.http.post<AlumnoPace>(`${this.url}/asignar`, { alumnoId, paceId });
  }

  checkProgress(alumnoPaceId: string, exitoso: boolean): Observable<void> {
    return this.http.post<void>(`${this.url}/${alumnoPaceId}/check`, { exitoso });
  }
}
