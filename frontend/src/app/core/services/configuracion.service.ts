import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ActualizarConfiguracionRequest, ConfiguracionPrivilegios } from '../../models';

@Injectable({ providedIn: 'root' })
export class ConfiguracionService {
  private readonly url = `${environment.apiUrl}/Configuracion`;

  constructor(private http: HttpClient) {}

  getPrivilegios(): Observable<ConfiguracionPrivilegios> {
    return this.http.get<ConfiguracionPrivilegios>(`${this.url}/privilegios`);
  }

  updatePrivilegios(request: ActualizarConfiguracionRequest): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.url}/privilegios`, request);
  }
}
