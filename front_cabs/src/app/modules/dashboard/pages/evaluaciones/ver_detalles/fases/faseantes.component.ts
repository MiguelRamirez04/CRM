// =====================================================================================
// COMPONENTE FASE ANTES - REFACTORIZADO CON SIDE PANEL
// =====================================================================================

import { Component, OnInit, OnChanges, SimpleChanges, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { EvaluacionService } from '../../../../../../core/services/evaluaciones.service';
import { 
  EvaluacionDetalleResponse, 
  FotoEvaluacionResponse 
} from '../../../../../../core/models/evaluaciones.interface';
import { FaseEvaluacion } from '../../../../../../core/enums/evaluaciones.enum';

// Importar componentes reutilizables
import { SidePanelComponent } from '../../../../../../shared/organisms/side-panel/side-panel.component';
import { DetailSectionComponent } from '../../../../../../shared/molecules/detail-section/detail-section.component';
import { DetailFieldComponent } from '../../../../../../shared/molecules/detail-field/detail-field.component';
import { BadgeComponent } from '../../../../../../shared/atoms/bage/badge.component';
import { AlertComponent } from '../../../../../../shared/molecules/alert/alert.component';
import { LoadingSpinnerComponent } from '../../../../../../shared/atoms/loading-spinner/loading-spinner.component';
import { UiBotonComponent } from '../../../../../../shared/atoms/boton/boton.component';

interface FotoConDetalle extends FotoEvaluacionResponse {
  urlImagen?: string;
}

@Component({
  selector: 'app-faseantes',
  standalone: true,
  imports: [
    CommonModule,
    SidePanelComponent,
    DetailSectionComponent,
    DetailFieldComponent,
    BadgeComponent,
    AlertComponent,
    LoadingSpinnerComponent,
    UiBotonComponent
  ],
  templateUrl: './fases.component.html',
  styleUrls: ['./fases.component.css']
})
export class FaseantesComponent implements OnInit, OnChanges {
  // =====================================================================================
  // INPUTS Y OUTPUTS
  // =====================================================================================
  
  @Input() evaluacionId: number = 0;
  @Input() mostrarPanel: boolean = false;
  @Input() soloContenido: boolean = false;
  @Output() cerrar = new EventEmitter<void>();
  
  // =====================================================================================
  // PROPIEDADES
  // =====================================================================================
  
  datosFase?: EvaluacionDetalleResponse;
  fotos: FotoConDetalle[] = [];
  
  cargando = true;
  error?: string;

  // 🔥 NUEVO: Mapa para almacenar blob URLs
  private blobUrls: Map<number, string> = new Map();

  // Exponer FASE para que el template pueda acceder
  readonly FASE = FaseEvaluacion.ANTES;

  // Iconos SVG
  readonly ICONOS = {
    info: 'M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z',
    documento: 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z',
    lightbulb: 'M12 18v-5.25m0 0a6.01 6.01 0 001.5-.189m-1.5.189a6.01 6.01 0 01-1.5-.189m3.75 7.478a12.06 12.06 0 01-4.5 0m3.75 2.383a14.406 14.406 0 01-3 0M14.25 18v-.192c0-.983.658-1.823 1.508-2.316a7.5 7.5 0 10-7.517 0c.85.493 1.509 1.333 1.509 2.316V18',
    clipboard: 'M9 12h3.75M9 15h3.75M9 18h3.75m3 .75H18a2.25 2.25 0 002.25-2.25V6.108c0-1.135-.845-2.098-1.976-2.192a48.424 48.424 0 00-1.123-.08m-5.801 0c-.065.21-.1.433-.1.664 0 .414.336.75.75.75h4.5a.75.75 0 00.75-.75 2.25 2.25 0 00-.1-.664m-5.8 0A2.251 2.251 0 0113.5 2.25H15c1.012 0 1.867.668 2.15 1.586m-5.8 0c-.376.023-.75.05-1.124.08C9.095 4.01 8.25 4.973 8.25 6.108V8.25m0 0H4.875c-.621 0-1.125.504-1.125 1.125v11.25c0 .621.504 1.125 1.125 1.125h9.75c.621 0 1.125-.504 1.125-1.125V9.375c0-.621-.504-1.125-1.125-1.125H8.25zM6.75 12h.008v.008H6.75V12zm0 3h.008v.008H6.75V15zm0 3h.008v.008H6.75V18z',
    photo: 'M2.25 15.75l5.159-5.159a2.25 2.25 0 013.182 0l5.159 5.159m-1.5-1.5l1.409-1.409a2.25 2.25 0 013.182 0l2.909 2.909m-18 3.75h16.5a1.5 1.5 0 001.5-1.5V6a1.5 1.5 0 00-1.5-1.5H3.75A1.5 1.5 0 002.25 6v12a1.5 1.5 0 001.5 1.5zm10.5-11.25h.008v.008h-.008V8.25zm.375 0a.375.375 0 11-.75 0 .375.375 0 01.75 0z',
    chart: 'M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z',
    mapPin: 'M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z',
    calendar: 'M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z'
  };

  constructor(
    public evaluacionService: EvaluacionService,
    public router: Router
  ) {}

  // =====================================================================================
  // LIFECYCLE HOOKS
  // =====================================================================================

  ngOnInit(): void {
    console.log('🔵 FaseantesComponent ngOnInit', {
      evaluacionId: this.evaluacionId,
      mostrarPanel: this.mostrarPanel,
      soloContenido: this.soloContenido
    });

    // Cargar si está en modo embebido desde el inicio
    if (this.soloContenido && this.evaluacionId) {
      console.log('🔄 Cargando en modo embebido...');
      this.cargarDetallesPorEvaluacion();
    } else if (this.evaluacionId && this.mostrarPanel) {
      console.log('🔄 Cargando en modo standalone...');
      this.cargarDetallesPorEvaluacion();
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    console.log('🟡 FaseantesComponent ngOnChanges', changes);
    
    // Detectar cambios en evaluacionId
    if (changes['evaluacionId'] && !changes['evaluacionId'].firstChange) {
      if (this.evaluacionId) {
        console.log('🔄 Recargando por cambio en evaluacionId...');
        this.cargarDetallesPorEvaluacion();
      }
    }
    
    // Cargar cuando soloContenido cambia a true
    if (changes['soloContenido'] && changes['soloContenido'].currentValue && this.evaluacionId) {
      console.log('🔄 Recargando por cambio en soloContenido...');
      this.cargarDetallesPorEvaluacion();
    }
    
    // Cargar cuando mostrarPanel cambia (modo standalone)
    if (changes['mostrarPanel'] && changes['mostrarPanel'].currentValue && this.evaluacionId && !this.soloContenido) {
      console.log('🔄 Recargando por cambio en mostrarPanel...');
      this.cargarDetallesPorEvaluacion();
    }
  }

  ngOnDestroy(): void {
    // 🔥 NUEVO: Limpiar blob URLs al destruir componente
    this.limpiarBlobUrls();
  }

  // =====================================================================================
  // MÉTODOS DE CARGA
  // =====================================================================================

  private cargarDetallesPorEvaluacion(): void {
    if (!this.evaluacionId) {
      this.error = 'ID de evaluación no válido';
      this.cargando = false;
      return;
    }

    console.log('📡 Iniciando carga de detalles para evaluación:', this.evaluacionId);
    
    this.cargando = true;
    this.error = undefined;

    this.evaluacionService.obtenerDetallesPorEvaluacion(this.evaluacionId).subscribe({
      next: (detalles) => {
        console.log('✅ Detalles recibidos:', detalles);
        
        const detalleAntes = detalles.find(
          d => d.fase.toUpperCase() === this.FASE
        );
        
        if (detalleAntes) {
          console.log('✅ Detalle ANTES encontrado:', detalleAntes);
          this.datosFase = detalleAntes;
          this.cargarFotos();
        } else {
          console.warn('⚠️ No se encontró detalle ANTES');
          this.error = `No se encontró la evaluación de la fase ${this.FASE} para esta evaluación`;
          this.cargando = false;
        }
      },
      error: (err) => {
        console.error('❌ Error al cargar detalles:', err);
        this.error = 'Error al cargar los datos de la evaluación. Por favor, intente nuevamente.';
        this.cargando = false;
      }
    });
  }

  // 🔥 NUEVO: Cargar imagen desde el servidor con autenticación JWT
  private async cargarImagenAutenticada(fotoIdBD: number): Promise<string> {
    try {
      // Si ya está en caché, retornar
      if (this.blobUrls.has(fotoIdBD)) {
        return this.blobUrls.get(fotoIdBD)!;
      }

      console.log(`📸 Cargando imagen autenticada: ${fotoIdBD}`);
      
      const blob = await this.evaluacionService.descargarFoto(fotoIdBD).toPromise();
      
      if (!blob) {
        throw new Error('No se recibió blob');
      }

      const blobUrl = URL.createObjectURL(blob);
      this.blobUrls.set(fotoIdBD, blobUrl);
      
      console.log(`✅ Imagen ${fotoIdBD} cargada correctamente`);
      return blobUrl;
      
    } catch (error) {
      console.error(`❌ Error al cargar imagen ${fotoIdBD}:`, error);
      return '/assets/images/placeholder-image.png';
    }
  }

  // 🔥 ACTUALIZADO: Cargar fotos con autenticación
  private async cargarFotos(): Promise<void> {
    if (!this.datosFase?.id) {
      this.cargando = false;
      return;
    }

    console.log('📸 Cargando fotos para detalle:', this.datosFase.id);

    this.evaluacionService.obtenerTodasFotos().subscribe({
      next: async (todasLasFotos) => {
        const fotosFiltradas = todasLasFotos.filter(foto => foto.detalleId === this.datosFase!.id);

        console.log('✅ Fotos encontradas:', fotosFiltradas.length);

        // 🔥 Crear fotos SIN url, cargaremos después
        this.fotos = fotosFiltradas.map(foto => ({
          ...foto,
          urlImagen: undefined // Temporalmente sin URL
        }));

        // 🔥 Cargar las imágenes autenticadas
        await this.cargarPreviewsFotos();
        
        this.cargando = false;
      },
      error: (err) => {
        console.error('❌ Error al cargar fotos:', err);
        this.fotos = [];
        this.cargando = false;
      }
    });
  }

  // 🔥 NUEVO: Cargar previews de todas las fotos
  private async cargarPreviewsFotos(): Promise<void> {
    console.log(`📸 Cargando ${this.fotos.length} imágenes autenticadas...`);
    
    for (const foto of this.fotos) {
      if (foto.id) {
        foto.urlImagen = await this.cargarImagenAutenticada(foto.id);
      }
    }
    
    console.log('✅ Todas las imágenes cargadas');
  }

  // 🔥 NUEVO: Limpiar blob URLs para evitar memory leaks
  private limpiarBlobUrls(): void {
    this.blobUrls.forEach((url) => {
      URL.revokeObjectURL(url);
    });
    this.blobUrls.clear();
    console.log('🧹 Blob URLs limpiadas');
  }

  // 🔥 NUEVO: Manejo de error de imagen
  async onImageError(event: Event, foto: FotoConDetalle): Promise<void> {
    const imgElement = event.target as HTMLImageElement;
    
    console.error('❌ Error al cargar imagen:', {
      fotoId: foto.id
    });
    
    if (foto.id) {
      try {
        console.log('🔄 Reintentando con autenticación...');
        const blobUrl = await this.cargarImagenAutenticada(foto.id);
        foto.urlImagen = blobUrl;
        imgElement.src = blobUrl;
      } catch (error) {
        console.error('❌ Falló el reintento:', error);
        imgElement.src = '/assets/images/placeholder-image.png';
      }
    } else {
      imgElement.src = '/assets/images/placeholder-image.png';
    }
  }

  // =====================================================================================
  // MÉTODOS DE NAVEGACIÓN
  // =====================================================================================

  cerrarPanel(): void {
    this.cerrar.emit();
  }

  volverADetalles(): void {
    this.cerrarPanel();
  }

  reintentar(): void {
    this.cargarDetallesPorEvaluacion();
  }

  // =====================================================================================
  // GETTERS
  // =====================================================================================

  get scoreWidth(): string {
    const score = this.datosFase?.scoreFase;
    if (score === null || score === undefined) return '0%';
    return `${Math.max(0, Math.min(100, score))}%`;
  }

  get claseScore(): string {
    const score = this.datosFase?.scoreFase;
    if (score === null || score === undefined) return 'bg-gray-300 text-black';
    return this.evaluacionService.obtenerClaseScore(score);
  }

  formatearFecha(fecha: Date | string | undefined): string {
    if (!fecha) return 'N/A';
    
    try {
      const fechaISO = typeof fecha === 'string' ? fecha : fecha.toISOString();
      return this.evaluacionService.formatearFecha(fechaISO);
    } catch (error) {
      return 'Fecha inválida';
    }
  }

  get labelFase(): string {
    return this.evaluacionService.obtenerLabelFase(this.FASE);
  }

  get tieneFotos(): boolean {
    return this.fotos.length > 0;
  }

  get cantidadFotos(): number {
    return this.fotos.length;
  }

  obtenerTipoBadgeScore(score: number | null | undefined): 'success' | 'info' | 'warning' | 'error' | 'default' {
    if (score === null || score === undefined) return 'default';
    if (score >= 80) return 'success';
    if (score >= 60) return 'info';
    if (score >= 40) return 'warning';
    return 'error';
  }

  get tipoBadgeFase(): 'success' | 'info' | 'warning' | 'error' | 'default' {
    return 'info'; // ANTES es azul (info)
  }
}