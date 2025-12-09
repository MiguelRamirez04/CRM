import { Routes } from '@angular/router';

export const recepcionRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('../../layout/dashboard-layout/dashboard-layout.component').then(m => m.DashboardLayoutComponent),
    children: [
      {
        path: 'clientes-completos',
        loadComponent: () => import('./pages/clientes-completos/clientes-completos.component').then(m => m.ClientesCompletosComponent)
      },

    ]
  }
];