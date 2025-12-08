import { Routes } from '@angular/router';


export const modulesSharedRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('../../layout/dashboard-layout/dashboard-layout.component').then(m => m.DashboardLayoutComponent),
    children: [
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
        path: 'usuarios',
        loadComponent: () => import('./pages/usuarios/usuarios.component').then(m => m.UsuariosComponent)
      },
      {
        path: 'perfil',
        loadComponent: () => import('../modulesShared/pages/profile/profile.component').then(m => m.ProfileComponent)
      },
      {
        path: 'congiruacion',
        loadComponent: () => import('../modulesShared/pages/settings/settings.component').then(m => m.SettingsComponent)
      },            
    ]
  }
];
