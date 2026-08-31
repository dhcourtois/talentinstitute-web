import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { rolesDe } from './core/auth/permissions';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full',
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
    ],
  },
  {
    path: '**',
    redirectTo: 'dashboard',
  },
];
