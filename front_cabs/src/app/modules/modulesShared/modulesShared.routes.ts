import { Routes } from '@angular/router';


export const modulesSharedRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('../../layout/dashboard-layout/dashboard-layout.component').then(m => m.DashboardLayoutComponent),
    children: [
      {
        path: '',
        loadComponent: () => import('./pages/ejecuciones-orden/ejecuciones-orden.component').then(m => m.EjecucionesOrdenComponent)
      },
      {
        path: 'vehiculos',
        loadComponent: () => import('./pages/vehiculos/vehiculos.component').then(m => m.VehiculosComponent)
      },
      {
        path: 'vehiculos/historial/:id',
        loadComponent: () => import('./pages/vehiculos/vehiculo-historial/vehiculo-historial.component').then(m => m.VehiculoHistorialComponent)
      },
      {
        path: 'viaticos',
        loadComponent: () => import('./pages/viaticos/viaticos.component').then(m => m.ViaticosComponent)
      },
      {
        path: 'reparaciones',
        loadComponent: () => import('./pages/reparaciones/reparaciones.component').then(m => m.ReparacionesComponent)
      },
       {
        path: 'reparaciones/:id/componentes', 
        loadComponent: () => import('./pages/reparaciones/componentes/reparacion-componentes.component').then(m => m.ReparacionComponentesComponent)
      }
    ]
  }
];
