// =====================================================================================
// COMPONENTE MODAL - FASE ANTES
// =====================================================================================
//
// 🎯 Este componente se abre como un MODAL desde Info General
// No hay navegación de rutas
// No se destruye el componente padre
// Los datos persisten automáticamente
//
// =====================================================================================

import { Component, OnInit, OnDestroy, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { EvaluacionService } from '../../../../../../core/services/evaluaciones.service';
import { SharedEvaluacionService } from '../../../../../../core/services/shared-evaluacion.service';
import { DatosFase, FotoLocal } from '../../../../../../core/models/evaluaciones.interface';

@Component({
  selector: 'app-fase-antes-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './fases.component.html',
  styleUrls: ['./fases.component.css']
})
export class FaseAntesModalComponent implements OnInit, OnDestroy {
  @Input() modoOperacion: 'crear' | 'editar' = 'crear';
  @Input() evaluacionId: number | null = null;
  @Output() cerrar = new EventEmitter<void>();

  lugar: string = '';
  fechaCreacion: string = '';
  scoreFase: number = 0;
  descripcion: string = '';
  sugerencias: string = '';
  notaGeneral: string = '';
  fotos: FotoLocal[] = [];
  tituloFase: string = 'ANTES';
  guardando = false;
  detalleId?: number;

  private blobUrls: Map<number, string> = new Map();
  private destroy$ = new Subject<void>();

  constructor(
    private evaluacionService: EvaluacionService,
    private sharedService: SharedEvaluacionService
  ) {
    this.fechaCreacion = this.obtenerFechaActual();
  }

  ngOnInit(): void {
    console.log('🔓 Modal ANTES abierto');
    this.cargarDatosGuardados();
    this.suscribirCambios();
  }

  ngOnDestroy(): void {
    console.log('🔒 Modal ANTES cerrado - guardando datos');
    this.guardarDatosEnServicio();
    this.destroy$.next();
    this.destroy$.complete();
    this.limpiarBlobUrls();
  }

  /**
   * Cargar imagen autenticada
   */
  private async cargarImagenAutenticada(fotoIdBD: number): Promise<string> {
    try {
      if (this.blobUrls.has(fotoIdBD)) {
        return this.blobUrls.get(fotoIdBD)!;
      }

      const blob = await this.evaluacionService.descargarFoto(fotoIdBD).toPromise();
      
      if (!blob) {
        throw new Error('No se recibió blob');
      }

      const blobUrl = URL.createObjectURL(blob);
      this.blobUrls.set(fotoIdBD, blobUrl);
      
      return blobUrl;
      
    } catch (error) {
      console.error(`Error al cargar imagen ${fotoIdBD}:`, error);
      return '/assets/images/placeholder-image.png';
    }
  }

  /**
   * Cargar previews de fotos
   */
  private async cargarPreviewsFotos(): Promise<void> {
    const fotosConIdBD = this.fotos.filter(f => f.fotoIdBD && !f.preview?.startsWith('data:'));
    
    if (fotosConIdBD.length === 0) return;
    
    for (const foto of fotosConIdBD) {
      if (foto.fotoIdBD) {
        foto.preview = await this.cargarImagenAutenticada(foto.fotoIdBD);
      }
    }
  }

  /**
   * Limpiar blob URLs
   */
  private limpiarBlobUrls(): void {
    this.blobUrls.forEach((url) => {
      URL.revokeObjectURL(url);
    });
    this.blobUrls.clear();
  }

  private cargarDatosGuardados(): void {
    const datosGuardados = this.sharedService.getFaseAntes();
    if (datosGuardados) {
      this.lugar = datosGuardados.lugar || '';
      this.fechaCreacion = datosGuardados.fechaCreacion || this.obtenerFechaActual();
      this.scoreFase = datosGuardados.scoreFase || 0;
      this.descripcion = datosGuardados.descripcion || '';
      this.sugerencias = datosGuardados.sugerencias || '';
      this.notaGeneral = datosGuardados.notaGeneral || '';
      this.fotos = datosGuardados.fotos || [];
      this.detalleId = datosGuardados.detalleId;
      
      this.cargarPreviewsFotos();
      
      console.log('Datos ANTES cargados en modal');
    }
  }

  async onImageError(event: Event, foto: FotoLocal): Promise<void> {
    const imgElement = event.target as HTMLImageElement;
    
    if (foto.fotoIdBD) {
      try {
        const blobUrl = await this.cargarImagenAutenticada(foto.fotoIdBD);
        foto.preview = blobUrl;
        imgElement.src = blobUrl;
      } catch (error) {
        imgElement.src = '/assets/images/placeholder-image.png';
      }
    } else {
      imgElement.src = '/assets/images/placeholder-image.png';
    }
  }

  private suscribirCambios(): void {
    this.sharedService.guardando$
      .pipe(takeUntil(this.destroy$))
      .subscribe(guardando => {
        this.guardando = guardando;
      });
  }

  private guardarDatosEnServicio(): void {
    const datos: DatosFase = {
      detalleId: this.detalleId,
      lugar: this.lugar,
      fechaCreacion: this.fechaCreacion,
      scoreFase: this.scoreFase,
      descripcion: this.descripcion,
      sugerencias: this.sugerencias,
      notaGeneral: this.notaGeneral,
      fotos: this.fotos
    };
    this.sharedService.setFaseAntes(datos);
    this.sharedService.actualizarScore();
    console.log('Datos ANTES guardados en SharedService');
  }

  onCampoChange(): void {
    this.guardarDatosEnServicio();
  }

  obtenerFechaActual(): string {
    const hoy = new Date();
    return `${hoy.getFullYear()}-${String(hoy.getMonth() + 1).padStart(2, '0')}-${String(hoy.getDate()).padStart(2, '0')}`;
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      
      if (file.size > 10 * 1024 * 1024) {
        alert('Archivo demasiado grande. Máximo: 10MB');
        return;
      }

      const allowedTypes = ['image/jpeg', 'image/png', 'image/jpg', 'image/webp', 'image/gif'];
      if (!allowedTypes.includes(file.type)) {
        alert('Tipo no permitido. Use: JPG, PNG, WebP o GIF');
        return;
      }

      const nuevaFoto: FotoLocal = {
        id: this.generarId(),
        tipo: 'General',
        fecha: this.obtenerFechaActual(),
        descripcion: '',
        archivo: file
      };

      const reader = new FileReader();
      reader.onload = (e) => {
        if (e.target?.result) {
          nuevaFoto.preview = e.target.result as string;
        }
      };
      reader.readAsDataURL(file);

      this.fotos.push(nuevaFoto);
      this.onCampoChange();
    }
  }

  eliminarFoto(id: string | undefined): void {
    if (!id) return;
    const foto = this.fotos.find(f => f.id === id);
    if (!foto) return;
    
    if (foto.fotoIdBD) {
      if (!confirm('¿Eliminar esta foto? No se puede deshacer')) return;
      
      this.evaluacionService.eliminarFoto(foto.fotoIdBD).subscribe({
        next: () => {
          if (this.blobUrls.has(foto.fotoIdBD!)) {
            URL.revokeObjectURL(this.blobUrls.get(foto.fotoIdBD!)!);
            this.blobUrls.delete(foto.fotoIdBD!);
          }
          this.fotos = this.fotos.filter(f => f.id !== id);
          this.onCampoChange();
        },
        error: (err) => {
          console.error('Error al eliminar:', err);
          alert('Error al eliminar la foto');
        }
      });
    } else {
      this.fotos = this.fotos.filter(f => f.id !== id);
      this.onCampoChange();
    }
  }

  abrirSelectorArchivo(): void {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = 'image/*';
    input.onchange = (e) => this.onFileSelected(e);
    input.click();
  }

  private generarId(): string {
    return Date.now().toString() + Math.random().toString(36).substring(2, 9);
  }

  /**
   * Cerrar modal
   */
  cerrarModal(): void {
    this.guardarDatosEnServicio();
    this.cerrar.emit();
  }

  /**
   * Cerrar con ESC
   */
  onKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Escape') {
      this.cerrarModal();
    }
  }

  get modoEdicion(): boolean {
    return this.modoOperacion === 'editar';
  }
}