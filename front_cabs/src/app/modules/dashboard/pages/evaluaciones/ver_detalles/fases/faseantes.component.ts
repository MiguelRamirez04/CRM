import { Component, OnInit, OnChanges, SimpleChanges, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { EvaluacionService } from '../../../../../../core/services/evaluaciones.service';
import {
  EvaluacionDetalleResponse,
  FotoEvaluacionResponse
} from '../../../../../../core/models/evaluaciones.interface';
import { FaseEvaluacion } from '../../../../../../core/enums/evaluaciones.enum';

// Importar componentes reutilizables desde index
import {
  SidePanelComponent,
  DetailSectionComponent,
  BadgeComponent,
  AlertComponent,
  LoadingSpinnerComponent,
  UiBotonComponent,
  UitipografiaComponent,
  UiIconComponent
} from '../../../../../../shared/~exports/detail-view.index';

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
    BadgeComponent,
    AlertComponent,
    LoadingSpinnerComponent,
    UiBotonComponent,
    UitipografiaComponent,
    UiIconComponent
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

  // Mapa para almacenar blob URLs
  private blobUrls: Map<number, string> = new Map();

  // Exponer FASE para que el template pueda acceder
  readonly FASE = FaseEvaluacion.ANTES;


  constructor(
    public evaluacionService: EvaluacionService,
    public router: Router
  ) { }

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
      console.log('Cargando en modo embebido...');
      this.cargarDetallesPorEvaluacion();
    } else if (this.evaluacionId && this.mostrarPanel) {
      console.log('Cargando en modo standalone...');
      this.cargarDetallesPorEvaluacion();
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    console.log('FaseantesComponent ngOnChanges', changes);

    // Detectar cambios en evaluacionId
    if (changes['evaluacionId'] && !changes['evaluacionId'].firstChange) {
      if (this.evaluacionId) {
        console.log('Recargando por cambio en evaluacionId...');
        this.cargarDetallesPorEvaluacion();
      }
    }

    // Cargar cuando soloContenido cambia a true
    if (changes['soloContenido'] && changes['soloContenido'].currentValue && this.evaluacionId) {
      console.log('Recargando por cambio en soloContenido...');
      this.cargarDetallesPorEvaluacion();
    }

    // Cargar cuando mostrarPanel cambia (modo standalone)
    if (changes['mostrarPanel'] && changes['mostrarPanel'].currentValue && this.evaluacionId && !this.soloContenido) {
      console.log('Recargando por cambio en mostrarPanel...');
      this.cargarDetallesPorEvaluacion();
    }
  }

  ngOnDestroy(): void {
    // Limpiar blob URLs al destruir componente
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
        console.log(' Detalles recibidos:', detalles);

        const detalleAntes = detalles.find(
          d => d.fase.toUpperCase() === this.FASE
        );

        if (detalleAntes) {
          console.log(' Detalle ANTES encontrado:', detalleAntes);
          this.datosFase = detalleAntes;
          this.cargarFotos();
        } else {
          console.warn('⚠️ No se encontró detalle ANTES');
          this.error = `No se encontró la evaluación de la fase ${this.FASE} para esta evaluación`;
          this.cargando = false;
        }
      },
      error: (err) => {
        console.error('Error al cargar detalles:', err);
        this.error = 'Error al cargar los datos de la evaluación. Por favor, intente nuevamente.';
        this.cargando = false;
      }
    });
  }

  // Cargar imagen desde el servidor con autenticación JWT
  private async cargarImagenAutenticada(fotoIdBD: number): Promise<string> {
    try {
      // Si ya está en caché, retornar
      if (this.blobUrls.has(fotoIdBD)) {
        return this.blobUrls.get(fotoIdBD)!;
      }

      console.log(`Cargando imagen autenticada: ${fotoIdBD}`);

      const blob = await this.evaluacionService.descargarFoto(fotoIdBD).toPromise();

      if (!blob) {
        throw new Error('No se recibió blob');
      }

      const blobUrl = URL.createObjectURL(blob);
      this.blobUrls.set(fotoIdBD, blobUrl);

      console.log(` Imagen ${fotoIdBD} cargada correctamente`);
      return blobUrl;

    } catch (error) {
      console.error(`Error al cargar imagen ${fotoIdBD}:`, error);
      return '/assets/images/placeholder-image.png';
    }
  }
  abrirImagenEnNuevoTab(url: string): void {
    window.open(url, '_blank');
  }
  //ACTUALIZADO: Cargar fotos con autenticación
  private async cargarFotos(): Promise<void> {
    if (!this.datosFase?.id) {
      this.cargando = false;
      return;
    }

    console.log('Cargando fotos para detalle:', this.datosFase.id);

    this.evaluacionService.obtenerTodasFotos().subscribe({
      next: async (todasLasFotos) => {
        const fotosFiltradas = todasLasFotos.filter(foto => foto.detalleId === this.datosFase!.id);

        console.log(' Fotos encontradas:', fotosFiltradas.length);

        //Crear fotos SIN url, cargaremos después
        this.fotos = fotosFiltradas.map(foto => ({
          ...foto,
          urlImagen: undefined // Temporalmente sin URL
        }));

        //Cargar las imágenes autenticadas
        await this.cargarPreviewsFotos();

        this.cargando = false;
      },
      error: (err) => {
        console.error('Error al cargar fotos:', err);
        this.fotos = [];
        this.cargando = false;
      }
    });
  }

  // Cargar previews de todas las fotos
  private async cargarPreviewsFotos(): Promise<void> {
    console.log(`Cargando ${this.fotos.length} imágenes autenticadas...`);

    for (const foto of this.fotos) {
      if (foto.id) {
        foto.urlImagen = await this.cargarImagenAutenticada(foto.id);
      }
    }

    console.log(' Todas las imágenes cargadas');
  }

  // Limpiar blob URLs para evitar memory leaks
  private limpiarBlobUrls(): void {
    this.blobUrls.forEach((url) => {
      URL.revokeObjectURL(url);
    });
    this.blobUrls.clear();
    console.log('Blob URLs limpiadas');
  }

  // Manejo de error de imagen
  async onImageError(event: Event, foto: FotoConDetalle): Promise<void> {
    const imgElement = event.target as HTMLImageElement;

    console.error('Error al cargar imagen:', {
      fotoId: foto.id
    });

    if (foto.id) {
      try {
        console.log('Reintentando con autenticación...');
        const blobUrl = await this.cargarImagenAutenticada(foto.id);
        foto.urlImagen = blobUrl;
        imgElement.src = blobUrl;
      } catch (error) {
        console.error('Falló el reintento:', error);
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