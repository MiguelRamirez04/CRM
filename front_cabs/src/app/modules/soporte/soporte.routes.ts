import { Routes } from '@angular/router';

export const soporteRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/dashboard/dashboard.component').then(m => m.SoporteDashboardComponent)
  },
  {
    path: 'tickets',
    loadComponent: () => import('./pages/tickets/tickets.component').then(m => m.TicketsComponent)
  },
  {
    path: 'chat',
    loadComponent: () => import('./pages/chat/chat.component').then(m => m.ChatComponent)
  }
];