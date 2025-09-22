import { Routes } from '@angular/router';

export const administracionRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/dashboard/dashboard.component').then(m => m.AdministracionDashboardComponent)
  },
  {
    path: 'empleados',
    loadComponent: () => import('./pages/empleados/empleados.component').then(m => m.EmpleadosComponent)
  },
  {
    path: 'roles',
    loadComponent: () => import('./pages/roles/roles.component').then(m => m.RolesComponent)
  },
  {
    path: 'configuracion',
    loadComponent: () => import('./pages/configuracion/configuracion.component').then(m => m.ConfiguracionComponent)
  }
];