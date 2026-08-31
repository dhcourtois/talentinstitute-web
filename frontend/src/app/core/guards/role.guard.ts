import { inject } from '@angular/core';
import { CanActivateFn, Router, ActivatedRouteSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { rutaInicial } from '../auth/permissions';
import { Rol } from '../../models';

export const roleGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) {
    return router.createUrlTree(['/login']);
  }

  const allowedRoles: Rol[] = route.data['roles'] ?? [];
  if (allowedRoles.length === 0) {
    return true;
  }

  const userRole = auth.getRole();
  if (userRole && allowedRoles.includes(userRole)) {
    return true;
  }

  // Se envía al primer módulo que el rol sí puede ver: redirigir siempre a
  // /dashboard dejaba en bucle a quien tampoco tiene permiso sobre él.
  return router.createUrlTree([rutaInicial(userRole)]);
};
