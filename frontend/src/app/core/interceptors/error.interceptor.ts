import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { ToastService } from '../services/toast.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const toast = inject(ToastService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        // Token expirado o credenciales inválidas — solo redirigir si no estamos ya en el login
        if (!router.url.startsWith('/login')) {
          localStorage.removeItem('ti_token');
          router.navigate(['/login']);
        }
      } else if (error.status === 403) {
        toast.warning('No tienes permiso para realizar esta acción.');
      } else if (error.status >= 500) {
        toast.error('Ocurrió un error en el servidor. Intenta de nuevo.');
      }

      return throwError(() => error);
    })
  );
};
