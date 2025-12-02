import { Routes } from '@angular/router';

export const dashboardRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('../../layout/dashboard-layout/dashboard-layout.component').then(m => m.DashboardLayoutComponent),
    children: [
      {
        path: '',
        loadComponent: () => import('./pages/landing/landing.component').then(m => m.DashboardComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('./pages/profile/profile.component').then(m => m.ProfileComponent)
      },
      {
        path: 'settings',
        loadComponent: () => import('./pages/settings/settings.component').then(m => m.SettingsComponent)
      },
      {
        path: 'calendario',
        loadComponent: () => import('./pages/calendario/calendario.component').then(m => m.CalendarioComponent)
      },
      
      // =====================================================================================
      // RUTAS DE EVALUACIONES - OPTIMIZADAS CON MODALES
      // =====================================================================================
      
      // Listado de evaluaciones
      {
        path: 'evaluaciones',
        loadComponent: () => import('./pages/evaluaciones/listado/evaluaciones.component')
          .then(m => m.EvaluacionesComponent)
      },
      
      // Ver detalle de evaluación (readonly)
      {
        path: 'evaluaciones/ver/:id',
        loadComponent: () => import('./pages/evaluaciones/ver_detalles/verdetalles.component')
          .then(m => m.VerdetallesComponent)
      },
      
      // Ver fase ANTES en detalle (readonly)
      {
        path: 'evaluacion/fase-antes/:id',
        loadComponent: () => import('./pages/evaluaciones/ver_detalles/fases/faseantes.component')
          .then(m => m.FaseantesComponent)
      },
      
      // Ver fase DESPUÉS en detalle (readonly)
      {
        path: 'evaluacion/fase-despues/:id',
        loadComponent: () => import('./pages/evaluaciones/ver_detalles/fases/fasedespues.component')
          .then(m => m.FasedespuesComponent)
      },
      
      // =====================================================================================
      // MODO CREAR - SOLO INFO GENERAL (FASES SON MODALES)
      // =====================================================================================
      
      {
        path: 'evaluaciones/nueva',
        loadComponent: () => import('./pages/evaluaciones/registro/infogeneral/infogeneralregistro.component')
          .then(m => m.InfogeneralComponent),
        data: { modo: 'crear' }
      },
      
    
      
      // =====================================================================================
      //  MODO EDITAR - SOLO INFO GENERAL (FASES SON MODALES)
      // =====================================================================================
      
      {
        path: 'evaluaciones/editar/:id',
        loadComponent: () => import('./pages/evaluaciones/registro/infogeneral/infogeneralregistro.component')
          .then(m => m.InfogeneralComponent),
        data: { modo: 'editar' }
      },
      
  
      
      // Redirect legacy
      {
        path: 'evaluaciones/registro',
        redirectTo: 'evaluaciones/nueva',
        pathMatch: 'full'
      },
      
      // =====================================================================================
      // RUTAS DE COTIZACIONES
      // =====================================================================================
      
      {
        path: 'cotizaciones',
        children: [
          {
            path: '',
            redirectTo: 'vista',
            pathMatch: 'full'
          },
          {
            path: 'nueva',
            loadComponent: () => import('./pages/cotizaciones/cotizacion.component').then(m => m.CotizacionComponent)
          },
          {
            path: 'vista',
            loadComponent: () => import('./pages/cotizaciones-vista/cotizaciones-vista.component').then(m => m.CotizacionesVistaComponent)
          },
          {
            path: 'editar/:id',
            loadComponent: () => import('./pages/cotizaciones/cotizacion.component').then(m => m.CotizacionComponent)
          }
        ]
      },
      
      // =====================================================================================
      // RUTAS DE CENTRO DE AYUDA
      // =====================================================================================
      
      {
        path: 'centrodeayuda',
        loadComponent: () => import('./pages/centroayuda/centroayuda.component').then(m => m.CentroayudaComponent)
      }
    ]
  }
];

