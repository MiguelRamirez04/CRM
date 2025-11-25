import { Routes } from '@angular/router';
import { SecureAuthGuard, AdminGuard, GuestGuard } from './core/guards/secure-auth.guard';

export const routes: Routes = [
  // Rutas públicas (solo para usuarios no autenticados)
  {
    path: 'auth',
    canActivate: [GuestGuard],
    loadChildren: () => import('./features/auth/auth.routes').then(m => m.authRoutes)
  },
  
  // Rutas protegidas (requieren autenticación)
  {
    path: 'dashboard',
    canActivate: [SecureAuthGuard],
    loadChildren: () => import('./modules/dashboard/dashboard.routes').then(m => m.dashboardRoutes)
  },

    {
    path: 'modulesShared',
    canActivate: [SecureAuthGuard],
    loadChildren: () => import('./modules/modulesShared/modulesShared.routes').then(m => m.modulesSharedRoutes)
  },
  
  // Administración (solo admins)
  {
    path: 'administracion',
    canActivate: [SecureAuthGuard,],
    data: { permission: 'administracion.read' },
    loadChildren: () => import('./modules/administracion/administracion.routes').then(m => m.administracionRoutes)
  },
  
  // Recepción (usuarios autenticados)
  {
    path: 'recepcion',
    canActivate: [SecureAuthGuard],
    data: { permission: 'recepcion.read' },
    loadChildren: () => import('./modules/recepcion/recepcion.routes').then(m => m.recepcionRoutes)
  },
  
  // Soporte (usuarios autenticados)
  {
    path: 'soporte',
    canActivate: [SecureAuthGuard],
    data: { permission: 'soporte.read' },
    loadChildren: () => import('./modules/soporte/soporte.routes').then(m => m.soporteRoutes)
  },
   {
    path: 'legacy',
    canActivate: [SecureAuthGuard], // Lo protegemos
    data: { permission: 'administracion.read' }, // Usamos el mismo permiso que admin
    loadChildren: () => import('./modules/legacy/legacy.routes').then(m => m.legacyRoutes)
  },
  
  // Viáticos (usuarios autenticados)
  {
    path: 'viaticos',
    canActivate: [SecureAuthGuard],
    loadComponent: () => import('./modules/modulesShared/pages/viaticos/viaticos.component').then(m => m.ViaticosComponent)
  },
  
  // Páginas de error
  {
    path: 'unauthorized',
    loadComponent: () => import('./shared/components/unauthorized/unauthorized.component').then(m => m.UnauthorizedComponent)
  },
  
  // Redirecciones
  { path: '', redirectTo: '/auth/login', pathMatch: 'full' },
  { path: '**', redirectTo: '/auth/login' }
];
