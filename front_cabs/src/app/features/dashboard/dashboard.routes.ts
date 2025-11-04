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
      //Rutas de EVALUACIONES
      {
        path: 'evaluaciones',
        loadComponent: () => import('./pages/evaluaciones/listado/evaluaciones.component').then(m => m.EvaluacionesComponent)
      },
      {
        path: 'evaluacion/:id',
        loadComponent: () =>
          import('./pages/evaluaciones/ver_detalles/verdetalles.component').then(m => m.VerdetallesComponent)
      },
      {
        path: 'evaluacion/fase-antes/:evaluacionId',
        loadComponent: () => import('./pages/evaluaciones/ver_detalles/fases/faseantes.component').then(m => m.FaseantesComponent)
      },
      {
        path: 'evaluacion/fase-despues/:evaluacionId',
        loadComponent: () => import('./pages/evaluaciones/ver_detalles/fases/fasedespues.component').then(m => m.FasedespuesComponent)
      },
      {
        path: 'evaluaciones/registro',
        loadComponent: () => import('./pages/evaluaciones/registro/infogeneral/infogeneralregistro.component').then(m => m.InfogeneralregistroComponent)
      },
      {
        path: 'evaluaciones/registro/fase-antes',
        loadComponent: () => import('./pages/evaluaciones/registro/fases/faseantesregistro.component').then(m => m.FaseAntesRegistroComponent)
      },
      {
        path: 'evaluaciones/registro/fase-despues',
        loadComponent: () => import('./pages/evaluaciones/registro/fases/fasedespuesregistro.component').then(m => m.FaseDespuesRegistroComponent)
      },
      {
        path: 'evaluaciones/editar/:id',
        loadComponent: () => import('./pages/evaluaciones/registro/editar-evaluacion.component').then(m => m.EditarEvaluacionComponent)
      },
      // RUTAS DE COTIZACIONES
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
      // RUTAS DE CENTROAYUDA
      {
        path: 'centrodeayuda',
        loadComponent: () => import('./pages/centroayuda/centroayuda.component').then(m => m.CentroayudaComponent)
      }
    ]
  }
];
