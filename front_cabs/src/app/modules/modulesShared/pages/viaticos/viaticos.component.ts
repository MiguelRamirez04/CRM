import { Component, inject, OnInit, TemplateRef, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

// Servicios
import { GastoViaticoService } from '../../../../core/services/gasto-viatico.service';

// Interfaces
import {
  GastoViaticoResponse,
  GastoViaticoPaginatedResponse,
  GastoViaticoFiltros
} from '../../../../core/models/gasto-viatico.interface';

// Componentes Reutilizables
import { BuscadorFiltroComponent, ConfiguracionBuscador } from '../../../../shared/components/buscador-filtro/buscador-filtro.component';
import { ModalFiltrosComponent, ConfiguracionModalFiltros, ResultadoFiltros } from '../../../../shared/components/modal-filtros/modal-filtros.component';
import { TablaListadoComponent, ConfiguracionColumna, AccionTabla } from '../../../../shared/molecules/tabla-base/tabla-listado.component';
import { PaginacionComponent, ConfiguracionPaginacion } from '../../../../shared/components/paginacion/paginacion.component';
import { StatusDotComponent } from '../../../../shared/atoms/status-dot/status-dot.component';
import { UiHeaderComponent } from '../../../../shared/molecules/header/header.component';
// Componentes Específicos
import { ViaticosDialogComponent } from './viaticos-dialog/viaticos-dialog.component';
import { ViaticosDetalleComponent } from './viaticos-detalle/viaticos-detalle.component';

@Component({
  selector: 'app-viaticos',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    BuscadorFiltroComponent,
    ModalFiltrosComponent,
    TablaListadoComponent,
    PaginacionComponent,
    StatusDotComponent,
    ViaticosDialogComponent,
    ViaticosDetalleComponent,
    UiHeaderComponent
  ],
  templateUrl: './viaticos.component.html',
  styleUrls: ['./viaticos.component.css']
})
export class ViaticosComponent implements OnInit, AfterViewInit {
  // =====================================================================================
  // SERVICIOS E INYECCIONES
  // =====================================================================================
  private router = inject(Router);
  private viaticoService = inject(GastoViaticoService);

  // =====================================================================================
  // REFERENCIAS A TEMPLATES (solo para celdas personalizadas)
  // =====================================================================================
  @ViewChild('plantillaOrden') plantillaOrden!: TemplateRef<any>;
  @ViewChild('plantillaGastos') plantillaGastos!: TemplateRef<any>;
  @ViewChild('plantillaKM') plantillaKM!: TemplateRef<any>;
  @ViewChild('plantillaMonto') plantillaMonto!: TemplateRef<any>;
  @ViewChild('plantillaFactura') plantillaFactura!: TemplateRef<any>;

  // ICONOS COMO STRINGS DE SVG (ya no como TemplateRef)
  private readonly iconoVerSVG = `
    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
      <path stroke-linecap="round" stroke-linejoin="round" d="M2.036 12.322a1.012 1.012 0 0 1 0-.639C3.423 7.51 7.36 4.5 12 4.5c4.638 0 8.573 3.007 9.963 7.178.07.207.07.431 0 .639C20.577 16.49 16.64 19.5 12 19.5c-4.638 0-8.573-3.007-9.963-7.178Z" />
      <path stroke-linecap="round" stroke-linejoin="round" d="M15 12a3 3 0 1 1-6 0 3 3 0 0 1 6 0Z" />
    </svg>
  `;

  private readonly iconoEditarSVG = `
    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
      <path stroke-linecap="round" stroke-linejoin="round" d="m16.862 4.487 1.687-1.688a1.875 1.875 0 1 1 2.652 2.652L10.582 16.07a4.5 4.5 0 0 1-1.897 1.13L6 18l.8-2.685a4.5 4.5 0 0 1 1.13-1.897l8.932-8.931Zm0 0L19.5 7.125M18 14v4.75A2.25 2.25 0 0 1 15.75 21H5.25A2.25 2.25 0 0 1 3 18.75V8.25A2.25 2.25 0 0 1 5.25 6H10" />
    </svg>
  `;

  // =====================================================================================
  // DATOS
  // =====================================================================================
  viaticos: GastoViaticoResponse[] = [];
  Math = Math;

  // =====================================================================================
  // PAGINACIÓN
  // =====================================================================================
  paginaActual = 1;
  resultadosPorPagina = 10;
  totalPaginas = 0;
  totalItems = 0;

  // =====================================================================================
  // CONTROL DE UI
  // =====================================================================================
  cargando = false;
  mensajeExito = '';
  mensajeError = '';
  mostrarDialog = false;
  viaticoEditando: GastoViaticoResponse | null = null;
  mostrarDetalle = false;
  viaticoDetalleId: number | null = null;
  mostrarModalFiltros = false;

  // =====================================================================================
  // BÚSQUEDA Y FILTROS
  // =====================================================================================
  terminoBusqueda = '';
  filtrosActuales?: ResultadoFiltros;

  // Filtros individuales (para el backend)
  filtroOrdenId: number | null = null;
  filtroFechaDesde: string = '';
  filtroFechaHasta: string = '';
  filtroTieneFactura: boolean | null = null;

  // Variables de control para filtrado local
  private filtrandoPorDestino = false;
  private todosLosViaticos: GastoViaticoResponse[] = [];

  // =====================================================================================
  // CONFIGURACIONES DE COMPONENTES REUTILIZABLES
  // =====================================================================================

  /** Configuración del Buscador */
  configuracionBuscador: ConfiguracionBuscador = {
    placeholderBusqueda: 'Buscar por ID o destino',
    mostrarBotonFiltro: true,
    mostrarBotonNuevo: true,
    textoBotonNuevo: 'Nuevo Viático',
    tiempoEsperaBusqueda: 300
  };

  /** Configuración del Modal de Filtros */
  configuracionFiltros: ConfiguracionModalFiltros = {
    titulo: 'Filtros',
    gruposCheckbox: [
      {
        id: 'tieneFactura',
        titulo: 'Estado de Factura',
        opciones: [
          { valor: true, etiqueta: 'Con Factura', descripcion: 'Viáticos que tienen factura', seleccionado: false },
          { valor: false, etiqueta: 'Sin Factura', descripcion: 'Viáticos sin factura', seleccionado: false }
        ]
      }
    ],
    filtrosFecha: [
      {
        id: 'fechaDesde',
        titulo: 'Fecha Desde',
        placeholder: 'Seleccionar fecha desde',
        tipo: 'date'
      },
      {
        id: 'fechaHasta',
        titulo: 'Fecha Hasta',
        placeholder: 'Seleccionar fecha hasta',
        tipo: 'date'
      }
    ],
    mostrarBotonLimpiar: true,
    textoBotonAplicar: 'Aplicar',
    textoBotonLimpiar: 'Limpiar',
    textoBotonCerrar: 'Cerrar'
  };

  /** Configuración de Paginación */
  configuracionPaginacion: ConfiguracionPaginacion = {
    elementosPorPagina: 10,
    paginasVisiblesMaximas: 6,
    textoAnterior: 'Atrás',
    textoSiguiente: 'Siguiente',
    textoMostrandoRegistros: 'Visualizando',
    textoDeRegistros: 'de',
    mostrarInfoRegistros: true,
    mostrarBotonesPagina: true
  };

  /** Configuración de Columnas de Tabla */
  columnasTabla: ConfiguracionColumna<GastoViaticoResponse>[] = [];

  /** Acciones de Tabla */
  accionesTabla: AccionTabla<GastoViaticoResponse>[] = [];

  // =====================================================================================
  // LIFECYCLE HOOKS
  // =====================================================================================

  ngOnInit(): void {
    this.inicializarAccionesTabla();
    this.cargarViaticos();
  }

  ngAfterViewInit(): void {
    // Inicializar columnas después de que las plantillas estén disponibles
    this.inicializarColumnasTabla();
  }

  // =====================================================================================
  // INICIALIZACIÓN DE TABLA
  // =====================================================================================

  private inicializarColumnasTabla(): void {
    this.columnasTabla = [
      {
        encabezado: 'ID',
        campo: 'id',
        ancho: '80px',
        alineacion: 'center'
      },
      {
        encabezado: 'Fecha',
        campo: 'fecha',
        ancho: '120px',
        alineacion: 'center'
      },
      {
        encabezado: 'Orden',
        plantilla: this.plantillaOrden,
        ancho: '100px',
        alineacion: 'center'
      },
      {
        encabezado: 'Destino',
        campo: 'lugarDestino',
        ancho: '150px',
        alineacion: 'center'
      },
      {
        encabezado: 'Gastos',
        plantilla: this.plantillaGastos,
        ancho: '200px',
        alineacion: 'center'
      },
      {
        encabezado: 'KM',
        plantilla: this.plantillaKM,
        ancho: '80px',
        alineacion: 'center'
      },
      {
        encabezado: 'Monto',
        plantilla: this.plantillaMonto,
        ancho: '120px',
        alineacion: 'center'
      },
      {
        encabezado: 'Factura',
        plantilla: this.plantillaFactura,
        ancho: '200px',
        alineacion: 'center'
      }
    ];
  }

  private inicializarAccionesTabla(): void {
    // USAR STRINGS DE SVG EN LUGAR DE TEMPLATEREF
    this.accionesTabla = [
      {
        etiqueta: 'Ver Detalles',
        icono: this.iconoVerSVG,
        variante: 'primario',
        accion: (viatico: GastoViaticoResponse) => this.verDetalle(viatico)
      },
      {
        etiqueta: 'Editar',
        icono: this.iconoEditarSVG,
        variante: 'primario',
        accion: (viatico: GastoViaticoResponse) => this.editarViatico(viatico)
      }
    ];
  }

  // =====================================================================================
  // CARGA DE DATOS
  // =====================================================================================

  /**
   * Cargar viáticos con filtros y paginación
   */
  cargarViaticos(): void {
    // Si estamos filtrando por destino, usar el método específico
    if (this.filtrandoPorDestino && this.terminoBusqueda) {
      this.cargarViaticosFiltradosPorDestino(this.terminoBusqueda);
      return;
    }

    this.cargando = true;
    this.mensajeError = '';

    const filtros: GastoViaticoFiltros = {
      ordenId: this.filtroOrdenId ?? undefined,
      fechaDesde: this.filtroFechaDesde || undefined,
      fechaHasta: this.filtroFechaHasta || undefined,
      pageNumber: this.paginaActual,
      pageSize: this.resultadosPorPagina
    };

    this.viaticoService.obtenerViaticos(filtros).subscribe({
      next: (response: GastoViaticoPaginatedResponse) => {
        // Aplicar filtro de factura en el frontend si está activo
        let viaticosTemp = response.items;
        if (this.filtroTieneFactura !== null) {
          viaticosTemp = viaticosTemp.filter(v => v.tieneFactura === this.filtroTieneFactura);
        }

        this.viaticos = viaticosTemp;
        this.totalItems = response.totalItems ?? 0;
        this.totalPaginas = response.totalPaginas ?? 0;
        this.cargando = false;
      },
      error: (err) => {
        console.error('Error al cargar viáticos:', err);
        this.mensajeError = 'Error al cargar la lista de viáticos. Por favor, intente nuevamente.';
        this.cargando = false;
      }
    });
  }

  // =====================================================================================
  // GESTIÓN DE BÚSQUEDA
  // =====================================================================================

  /**
   * Handler para búsqueda
   */
  onBuscar(termino: string): void {
    this.terminoBusqueda = termino.trim();
    this.paginaActual = 1;

    // Si el término está vacío, limpiar búsqueda
    if (!this.terminoBusqueda) {
      this.filtroOrdenId = null;
      this.filtrandoPorDestino = false;
      this.todosLosViaticos = [];
      this.cargarViaticos();
      return;
    }

    // Si es un número, buscar por ordenId
    if (!isNaN(Number(this.terminoBusqueda))) {
      this.filtroOrdenId = Number(this.terminoBusqueda);
      this.filtrandoPorDestino = false;
      this.todosLosViaticos = [];
      this.cargarViaticos();
      return;
    }

    // Si no es un número, filtrar por destino
    this.filtroOrdenId = null;
    this.filtrandoPorDestino = true;
    this.cargarViaticosFiltradosPorDestino(this.terminoBusqueda);
  }

  /**
   * Cargar viáticos y filtrar por destino localmente
   */
  private cargarViaticosFiltradosPorDestino(termino: string): void {
    this.cargando = true;
    this.mensajeError = '';

    const filtros: GastoViaticoFiltros = {
      ordenId: undefined,
      fechaDesde: this.filtroFechaDesde || undefined,
      fechaHasta: this.filtroFechaHasta || undefined,
      pageNumber: 1,
      pageSize: 1000 // Obtener todos para filtrar localmente
    };

    this.viaticoService.obtenerViaticos(filtros).subscribe({
      next: (response: GastoViaticoPaginatedResponse) => {
        // Guardar todos los viáticos en cache
        this.todosLosViaticos = response.items;

        // Filtrar por destino (null-safe)
        let viaticosTemp = this.todosLosViaticos.filter(v =>
          v.lugarDestino?.toLowerCase().includes(termino.toLowerCase()) ?? false
        );

        // Aplicar filtro de factura si está activo
        if (this.filtroTieneFactura !== null) {
          viaticosTemp = viaticosTemp.filter(v => v.tieneFactura === this.filtroTieneFactura);
        }

        // Actualizar valores
        this.totalItems = viaticosTemp.length;
        this.totalPaginas = Math.ceil(viaticosTemp.length / this.resultadosPorPagina);

        // Aplicar paginación manual
        this.aplicarPaginacionLocal(viaticosTemp);

        this.cargando = false;
      },
      error: (err) => {
        console.error('Error al cargar viáticos:', err);
        this.mensajeError = 'Error al cargar la lista de viáticos. Por favor, intente nuevamente.';
        this.cargando = false;
      }
    });
  }

  /**
   * Aplicar paginación a datos locales
   */
  private aplicarPaginacionLocal(datos: GastoViaticoResponse[]): void {
    const inicio = (this.paginaActual - 1) * this.resultadosPorPagina;
    const fin = inicio + this.resultadosPorPagina;
    this.viaticos = datos.slice(inicio, fin);
  }

  // =====================================================================================
  // GESTIÓN DE FILTROS
  // =====================================================================================

  /**
   * Abrir modal de filtros
   */
  abrirModalFiltros(): void {
    this.mostrarModalFiltros = true;
  }

  /**
   * Cerrar modal de filtros
   */
  cerrarModalFiltros(): void {
    this.mostrarModalFiltros = false;
  }

  /**
   * Aplicar filtros desde el modal
   */
  aplicarFiltrosModal(resultado: ResultadoFiltros): void {
    console.log('Filtros recibidos:', resultado);

    this.filtrosActuales = resultado;

    // Procesar filtros de fecha
    this.filtroFechaDesde = resultado.fechas['fechaDesde'] || '';
    this.filtroFechaHasta = resultado.fechas['fechaHasta'] || '';

    // Procesar filtros de factura (checkbox)
    const filtrosFactura = resultado.checkboxes['tieneFactura'];
    if (filtrosFactura && filtrosFactura.length > 0) {
      if (filtrosFactura.length === 1) {
        this.filtroTieneFactura = filtrosFactura[0];
      } else {
        this.filtroTieneFactura = null;
      }
    } else {
      this.filtroTieneFactura = null;
    }

    this.paginaActual = 1;

    // Si hay búsqueda activa, recargar con búsqueda
    if (this.terminoBusqueda) {
      this.onBuscar(this.terminoBusqueda);
    } else {
      this.cargarViaticos();
    }
  }

  /**
   * Limpiar filtros desde el modal
   */
  limpiarFiltrosModal(): void {
    this.filtrosActuales = undefined;
    this.filtroOrdenId = null;
    this.filtroFechaDesde = '';
    this.filtroFechaHasta = '';
    this.filtroTieneFactura = null;
    this.terminoBusqueda = '';
    this.filtrandoPorDestino = false;
    this.todosLosViaticos = [];
    this.paginaActual = 1;
    this.cargarViaticos();
  }

  // =====================================================================================
  // GESTIÓN DE DIÁLOGOS
  // =====================================================================================

  /**
   * Mostrar dialog para crear nuevo viático
   */
  nuevoViatico(): void {
    this.viaticoEditando = null;
    this.mostrarDialog = true;
    this.mensajeExito = '';
    this.mensajeError = '';
  }

  /**
   * Mostrar dialog para editar viático existente
   */
  editarViatico(viatico: GastoViaticoResponse): void {
    this.viaticoEditando = viatico;
    this.mostrarDialog = true;
    this.mensajeExito = '';
    this.mensajeError = '';
  }

  /**
   * Cerrar dialog
   */
  cerrarDialog(): void {
    this.mostrarDialog = false;
    this.viaticoEditando = null;
  }

  /**
   * Manejar guardado exitoso desde el dialog
   */
  onViaticoGuardado(viatico: GastoViaticoResponse): void {
    if (this.viaticoEditando) {
      this.mensajeExito = `Viático #${viatico.id} actualizado exitosamente.`;
    } else {
      this.mensajeExito = `Viático #${viatico.id} creado exitosamente.`;
    }

    this.cerrarDialog();
    this.cargarViaticos();

    // Limpiar mensaje después de 5 segundos
    setTimeout(() => {
      this.mensajeExito = '';
    }, 5000);
  }

  // =====================================================================================
  // GESTIÓN DE PANEL DE DETALLES
  // =====================================================================================

  /**
   * Abrir panel de detalles
   */
  verDetalle(viatico: GastoViaticoResponse): void {
    this.viaticoDetalleId = viatico.id;
    this.mostrarDetalle = true;
  }

  /**
   * Cerrar panel de detalles
   */
  cerrarDetalle(): void {
    this.mostrarDetalle = false;
    this.viaticoDetalleId = null;
  }

  // =====================================================================================
  // PAGINACIÓN
  // =====================================================================================

  /**
   * Cambiar página
   */
  cambiarPagina(pagina: number): void {
    if (pagina >= 1 && pagina <= this.totalPaginas) {
      this.paginaActual = pagina;

      // Si estamos filtrando por destino, aplicar paginación local
      if (this.filtrandoPorDestino && this.todosLosViaticos.length > 0) {
        let viaticosTemp = this.todosLosViaticos.filter(v =>
          v.lugarDestino?.toLowerCase().includes(this.terminoBusqueda.toLowerCase()) ?? false
        );

        if (this.filtroTieneFactura !== null) {
          viaticosTemp = viaticosTemp.filter(v => v.tieneFactura === this.filtroTieneFactura);
        }

        this.aplicarPaginacionLocal(viaticosTemp);
      } else {
        // Paginación normal del backend
        this.cargarViaticos();
      }
    }
  }

  // =====================================================================================
  // UTILIDADES DE FORMATO
  // =====================================================================================

  /**
   * Formatear fecha para display
   */
  formatearFecha(fecha: string): string {
    return this.viaticoService.formatearFechaDisplay(fecha);
  }

  /**
   * Formatear monto
   */
  formatearMoneda(monto: number): string {
    return this.viaticoService.formatearMoneda(monto);
  }

  /**
   * Obtener label de factura
   */
  getLabelFactura(tieneFactura: boolean): string {
    return this.viaticoService.obtenerLabelFactura(tieneFactura);
  }

  /**
   * Obtener clase CSS para badge de factura
   */
  getClaseFactura(tieneFactura: boolean): string {
    return this.viaticoService.obtenerClaseFactura(tieneFactura);
  }
}
