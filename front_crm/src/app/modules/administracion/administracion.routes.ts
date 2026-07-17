import { Routes } from '@angular/router';

export const administracionRoutes: Routes = [
    {
    path: '',
    loadComponent: () => import('../../layout/dashboard-layout/dashboard-layout.component').then(m => m.DashboardLayoutComponent),
    children: [
      {
        path: 'panel-control',
        loadComponent: () => import('./pages/panel-control/panel-control.component').then(m => m.PanelControlComponent)
      },
    ]
  }
];