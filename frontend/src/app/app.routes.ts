import { inject } from '@angular/core';
import { CanActivateFn, Router, Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { AuthService } from './core/services/auth.service';
import { rolesDe, rutaInicial } from './core/auth/permissions';

/** Manda a cada rol a la primera pantalla que sí puede ver. */
const inicioSegunRol: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) return router.createUrlTree(['/login']);

  return router.createUrlTree([rutaInicial(auth.getRole())]);
};

export const routes: Routes = [
  {
    // El destino depende del rol: el padre de familia no tiene dashboard, así
    // que una redirección fija a /dashboard lo mandaría a una pantalla que el
    // guard le rebota (issue #8).
    path: '',
    pathMatch: 'full',
    canActivate: [inicioSegunRol],
    children: [],
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./features/login/login.component').then(m => m.LoginComponent),
  },
  {
    // Todas las pantallas autenticadas viven bajo el shell, que arma el menú
    // lateral a partir de la matriz de permisos del rol en sesión.
    path: '',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./shared/layout/shell.component').then(m => m.ShellComponent),
    children: [
      {
        path: 'dashboard',
        canActivate: [roleGuard],
        data: { roles: rolesDe('dashboard') },
        loadComponent: () =>
          import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent),
      },
      {
        path: 'alumnos',
        canActivate: [roleGuard],
        data: { roles: rolesDe('alumnos') },
        loadComponent: () =>
          import('./features/alumnos/alumnos.component').then(m => m.AlumnosComponent),
      },
      {
        path: 'alumno/:id',
        canActivate: [roleGuard],
        data: { roles: rolesDe('alumnos') },
        loadComponent: () =>
          import('./features/alumno/alumno.component').then(m => m.AlumnoComponent),
      },
      {
        path: 'paces',
        canActivate: [roleGuard],
        data: { roles: rolesDe('paces') },
        loadComponent: () =>
          import('./features/paces/paces.component').then(m => m.PacesComponent),
      },
      {
        path: 'entrevistas',
        canActivate: [roleGuard],
        data: { roles: rolesDe('entrevistas') },
        loadComponent: () =>
          import('./features/entrevistas/entrevistas.component').then(m => m.EntrevistasComponent),
      },
      {
        path: 'staff',
        canActivate: [roleGuard],
        data: { roles: rolesDe('staff') },
        loadComponent: () =>
          import('./features/staff/staff.component').then(m => m.StaffComponent),
      },
      {
        path: 'configuracion',
        canActivate: [roleGuard],
        data: { roles: rolesDe('configuracion') },
        loadComponent: () =>
          import('./features/configuracion/configuracion.component').then(m => m.ConfiguracionComponent),
      },
      {
        path: 'padres',
        canActivate: [roleGuard],
        data: { roles: rolesDe('padres') },
        loadComponent: () =>
          import('./features/padres/padres.component').then(m => m.PadresComponent),
      },
      {
        // Única pantalla del rol Padre (issue #8).
        path: 'portal',
        canActivate: [roleGuard],
        data: { roles: rolesDe('portal') },
        loadComponent: () =>
          import('./features/portal/portal.component').then(m => m.PortalComponent),
      },
    ],
  },
  {
    path: '**',
    canActivate: [inicioSegunRol],
    children: [],
  },
];
