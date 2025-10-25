import { Routes } from '@angular/router';

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
    path: 'bandeja',
    loadComponent: () => import('./pages/pedidos/bandeja-recepcion.component').then(m => m.BandejaRecepcionComponent)
  }
];