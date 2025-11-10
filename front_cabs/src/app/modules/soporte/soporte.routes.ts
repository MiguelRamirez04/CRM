import { Routes } from '@angular/router';
import { VehiculosComponent } from '../modulesShared/pages/vehiculos/vehiculos.component';

export const soporteRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/dashboard/dashboard.component').then(m => m.SoporteDashboardComponent)
  },

  {
    path: 'vehiculos',
    loadComponent: () => import('../modulesShared/pages/vehiculos/vehiculos.component').then(m => m.VehiculosComponent)
  },
  {
    path: 'vehiculos/historial/:id',
    loadComponent: () => import('../modulesShared/pages/vehiculos/vehiculo-historial/vehiculo-historial.component').then(m => m.VehiculoHistorialComponent)
  },
  {
    path: 'asignaciones',
    loadComponent: () => import('./pages/asignaciones/asignaciones.component').then(m => m.MisAsignacionesComponent)
  },
  {
    path: 'chat',
    loadComponent: () => import('./pages/chat/chat.component').then(m => m.ChatComponent)
  }
];