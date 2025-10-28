import { Routes } from '@angular/router';
import { DashboardComponent } from '../../features/dashboard/pages/landing/landing.component';

export const recepcionRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/dashboard/dashboard.component').then(m => m.RecepcionDashboardComponent)
  },
  {
    path: 'clientes',
    loadComponent: () => import('./pages/clientes/clientes.component').then(m => m.ClientesComponent)
  },
  {
    path: 'clientes-completos',
    loadComponent: () => import('./pages/clientes-completos/clientes-completos.component').then(m => m.ClientesCompletosComponent)
  },
  {
    path: 'ordenes-trabajo',
    loadComponent: () => import('./pages/dashboard/dashboard.component').then(m => m.RecepcionDashboardComponent)
  }

  ,
  {
    path: 'ordenes-trabajo/ejecuciones',
    loadComponent: () => import('../modulesShared/pages/ejecuciones-orden/ejecuciones-orden.component').then(m => m.EjecucionesOrdenComponent)
  }

];