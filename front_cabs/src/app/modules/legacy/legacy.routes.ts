import { Routes } from '@angular/router';

// 1. 👇 CORRECCIÓN: Arreglados los typos (.compont -> .component)
import { CatalogoMenuComponent } from './page/menu/catalogo-menu.compont';
import { ConfiguracionMenuComponent } from './page/menu/configuracion-menu.compont';
import { OperacionesMenuComponent } from './page/menu/operaciones-menu.component';

export const legacyRoutes: Routes = [
  {
    // 2. 👇 ESTRUCTURA: El Layout Principal envuelve a todos
    path: '',
    loadComponent: () => import('../../layout/dashboard-layout/dashboard-layout.component').then(m => m.DashboardLayoutComponent),
    children: [
      
      // --- MENÚ 1: CATÁLOGOS (Tabs) ---
      {
        path: 'catalogos-base',
        component: CatalogoMenuComponent,
        children: [
          { 
            path: 'monedas', 
            loadComponent: () => import('./page/monedas/monedas.component').then(m => m.MonedasComponent) 
          },
          { 
            path: 'agentes', 
            loadComponent: () => import('./page/agentes/agentes.component').then(m => m.AgentesComponent) 
          },
          { 
            path: 'almacenes', 
            loadComponent: () => import('./page/almacenes/almacenes.component').then(m => m.AlmacenesComponent) 
          },
          { 
            path: 'productos', 
            loadComponent: () => import('./page/productos/productos.component').then(m => m.ProductosComponent) 
          },
          // Redirección interna de Catálogos
          { path: '', redirectTo: 'monedas', pathMatch: 'full' }
        ]
      },

      // --- MENÚ 2: CONFIGURACIÓN (Tabs) ---
      {
        path: 'config-documentos',
        component: ConfiguracionMenuComponent,
        children: [
          { 
            path: 'documentos-modelo', 
            loadComponent: () => import('./page/documentos-modelo/documentos-modelo.component').then(m => m.DocumentosModeloComponent) 
          },
          { 
            path: 'conceptos', 
            loadComponent: () => import('./page/conceptos/conceptos.component').then(m => m.ConceptosComponent) 
          },
          { 
            path: 'numeros-serie', 
            loadComponent: () => import('./page/numeros-serie/numeros-serie.component').then(m => m.NumerosSerieComponent) 
          },
          // Redirección interna de Configuración
          { path: '', redirectTo: 'documentos-modelo', pathMatch: 'full' }
        ]
      },

      // --- MENÚ 3: OPERACIONES (Tabs) ---
      {
        path: 'operaciones',
        component: OperacionesMenuComponent,
        children: [
          { 
            path: 'documentos', 
            loadComponent: () => import('./page/documentos/documentos.component').then(m => m.DocumentosComponent) 
          },
          { 
            path: 'movimientos', 
            loadComponent: () => import('./page/movimientos/movimientos.component').then(m => m.MovimientosComponent) 
          },
          { 
            path: 'movimientos-serie', 
            loadComponent: () => import('./page/movimientos-serie/movimientos-serie.component').then(m => m.MovimientosSerieComponent) 
          },
          // Redirección interna de Operaciones
          { path: '', redirectTo: 'documentos', pathMatch: 'full' }
        ]
      },

      // 3. 👇 REDIRECCIÓN GLOBAL DEL MÓDULO
      // Esta debe ir DENTRO del children del Layout para que cargue el layout antes de redirigir
      {
        path: '',
        redirectTo: 'catalogos-base',
        pathMatch: 'full'
      }
    ]
  }
];