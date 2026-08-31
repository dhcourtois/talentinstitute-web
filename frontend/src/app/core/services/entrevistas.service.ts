import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { EntrevistaPadre, RegistrarEntrevistaRequest } from '../../models';

@Injectable({ providedIn: 'root' })
export class EntrevistasService {
  private readonly url = `${environment.apiUrl}/EntrevistasPadres`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<EntrevistaPadre[]> {
    return this.http.get<EntrevistaPadre[]>(this.url);
  }

  getById(id: string): Observable<EntrevistaPadre> {
    return this.http.get<EntrevistaPadre>(`${this.url}/${id}`);
  }

  create(request: RegistrarEntrevistaRequest): Observable<{ id: string; message: string }> {
    return this.http.post<{ id: string; message: string }>(this.url, request);
  }
}
