import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Rol, Staff } from '../../models';

@Injectable({ providedIn: 'root' })
export class StaffService {
  private readonly url = `${environment.apiUrl}/Staff`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Staff[]> {
    return this.http.get<Staff[]>(this.url);
  }

  create(email: string, password: string, rol: Rol): Observable<{ id: string; message: string }> {
    return this.http.post<{ id: string; message: string }>(this.url, { email, password, rol });
  }

  updateRol(staffId: string, rol: Rol): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.url}/${staffId}`, { rol });
  }

  deactivate(staffId: string): Observable<{ message: string }> {
    return this.http.patch<{ message: string }>(`${this.url}/${staffId}/desactivar`, {});
  }
}
