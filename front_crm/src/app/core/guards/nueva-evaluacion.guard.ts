
import { Injectable } from '@angular/core';
import { 
  CanActivate, 
  ActivatedRouteSnapshot, 
  RouterStateSnapshot,
  Router 
} from '@angular/router';
import { SharedEvaluacionService } from '../services/shared-evaluacion.service';

/**
 * Guard inteligente que protege las rutas de creación de evaluaciones
 * 
 * COMPORTAMIENTO:
 * - Limpia el servicio al entrar desde FUERA del flujo (ej: desde listado)
 * - NO limpia cuando navegas entre secciones del mismo formulario
 * - Detecta modo de operación (crear vs editar)
 * - Logs detallados para debugging
 */
@Injectable({
  providedIn: 'root'
})
export class NuevaEvaluacionGuard implements CanActivate {
  
  // Última ruta visitada (para detectar navegación interna)
  private ultimaRuta: string | null = null;
  
  // Rutas del flujo de evaluación
  private readonly RUTAS_FLUJO = [
    '/dashboard/evaluaciones/nueva',
    '/dashboard/evaluaciones/nueva/fase-antes',
    '/dashboard/evaluaciones/nueva/fase-despues'
  ];

  constructor(
    private sharedService: SharedEvaluacionService,
    private router: Router
  ) {
    console.log(' NuevaEvaluacionGuard inicializado');
  }

  /**
   * Método principal del guard
   */
  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): boolean {
    const rutaActual = state.url;
    const modoOperacion = this.detectarModoOperacion(route);
    
    console.log(' ============================================');
    console.log(' Guard: canActivate');
    console.log(' Ruta actual:', rutaActual);
    console.log(' Ruta anterior:', this.ultimaRuta);
    console.log(' Modo operación:', modoOperacion);
    
    // Si es modo EDITAR, nunca limpiar
    if (modoOperacion === 'editar') {
      console.log('📝 Modo EDITAR - manteniendo datos');
      this.ultimaRuta = rutaActual;
      console.log(' ============================================');
      return true;
    }
    
    // Detectar tipo de navegación
    const esNavegacionInterna = this.esNavegacionDentroDelFlujo(rutaActual);
    
    if (esNavegacionInterna) {
      console.log(' Navegación INTERNA - NO limpiando');
      console.log('Estado actual:', this.sharedService.obtenerResumenEstado());
    } else {
      console.log(' Entrada EXTERNA - Limpiando servicio');
      this.limpiarServicio(rutaActual);
    }
    
    // Actualizar última ruta
    this.ultimaRuta = rutaActual;
    console.log(' ============================================');
    
    return true;
  }

  /**
   * Detectar si estamos en modo crear o editar
   */
  private detectarModoOperacion(route: ActivatedRouteSnapshot): 'crear' | 'editar' {
    // Si la ruta tiene un parámetro 'id', es modo editar
    const evaluacionId = route.paramMap.get('id');
    
    if (evaluacionId && evaluacionId !== 'nueva') {
      return 'editar';
    }
    
    // Verificar también en rutas padre
    let currentRoute: ActivatedRouteSnapshot | null = route;
    while (currentRoute) {
      const id = currentRoute.paramMap.get('id');
      if (id && id !== 'nueva') {
        return 'editar';
      }
      currentRoute = currentRoute.parent;
    }
    
    return 'crear';
  }

  /**
   * Determinar si la navegación es dentro del flujo de evaluación
   */
  private esNavegacionDentroDelFlujo(rutaActual: string): boolean {
    // Si no hay última ruta, definitivamente es entrada externa
    if (!this.ultimaRuta) {
      return false;
    }
    
    // Verificar si AMBAS rutas pertenecen al flujo
    const rutaAnteriorEraDelFlujo = this.RUTAS_FLUJO.some(r => 
      this.ultimaRuta?.includes(r)
    );
    
    const rutaActualEsDelFlujo = this.RUTAS_FLUJO.some(r => 
      rutaActual.includes(r)
    );
    
    // Es navegación interna solo si AMBAS son del flujo
    const esInterna = rutaAnteriorEraDelFlujo && rutaActualEsDelFlujo;
    
    console.log('🔍 Análisis de navegación:', {
      rutaAnteriorEraDelFlujo,
      rutaActualEsDelFlujo,
      esInterna
    });
    
    return esInterna;
  }

  /**
   * Limpiar el servicio compartido
   */
  private limpiarServicio(rutaActual: string): void {
    console.log(' Iniciando limpieza desde Guard');
    
    // Mostrar estado antes de limpiar
    const estadoAntes = this.sharedService.obtenerResumenEstado();
    console.log('Estado antes de limpiar:', estadoAntes);
    
    // Advertir si hay datos sucios
    if (this.sharedService.tieneDatosSucios()) {
      console.warn(' Se detectaron datos sucios que serán limpiados');
    }
    
    // Limpiar
    this.sharedService.limpiar();
    
    // Verificar limpieza
    const estadoDespues = this.sharedService.obtenerResumenEstado();
    console.log(' Estado después de limpiar:', estadoDespues);
    
    if (!this.sharedService.estaLimpio()) {
      console.error(' ERROR: El servicio no se limpió correctamente');
    }
  }

  /**
   * Resetear el guard (útil para testing)
   */
  reset(): void {
    console.log(' Reseteando Guard');
    this.ultimaRuta = null;
  }

  /**
   * Verificar si una ruta pertenece al flujo
   */
  esRutaDelFlujo(ruta: string): boolean {
    return this.RUTAS_FLUJO.some(r => ruta.includes(r));
  }

  /**
   * Obtener estado del guard (útil para debugging)
   */
  obtenerEstado(): {
    ultimaRuta: string | null;
    rutasFlujo: string[];
  } {
    return {
      ultimaRuta: this.ultimaRuta,
      rutasFlujo: this.RUTAS_FLUJO
    };
  }
}

