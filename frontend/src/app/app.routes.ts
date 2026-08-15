import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

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
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent),
  },
  {
    path: 'alumno/:id',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Principal', 'Supervisora', 'Monitora'] },
    loadComponent: () =>
      import('./features/alumno/alumno.component').then(m => m.AlumnoComponent),
  },
  {
    path: '**',
    redirectTo: 'dashboard',
  },
];
