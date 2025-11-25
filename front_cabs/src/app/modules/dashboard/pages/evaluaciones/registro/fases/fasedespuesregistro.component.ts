import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Subject, from } from 'rxjs';
import { takeUntil, finalize } from 'rxjs/operators';
import { EvaluacionService } from '../../../../../../core/services/evaluaciones.service';
import { SharedEvaluacionService } from '../../../../../../core/services/shared-evaluacion.service';
import { DatosFase, FotoLocal, EvaluacionResponse } from '../../../../../../core/models/evaluaciones.interface';

@Component({
  selector: 'app-fase-despues',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './fases.component.html',
  styleUrls: ['./fases.component.css']
})
export class FaseDespuesComponent implements OnInit, OnDestroy {
  lugar: string = '';
  fechaCreacion: string = '';
  scoreFase: number = 0;
  descripcion: string = '';
  sugerencias: string = '';
  notaGeneral: string = '';
  fotos: FotoLocal[] = [];
  faseActiva: 'antes' | 'despues' | 'infogeneral' = 'despues';
  tituloFase: string = 'DESPUÉS';
  guardando = false;
  detalleId?: number;
  modoOperacion: 'crear' | 'editar' = 'crear';
  evaluacionId: number | null = null;

  private blobUrls: Map<number, string> = new Map();

  private destroy$ = new Subject<void>();

  constructor(
    private evaluacionService: EvaluacionService,
    private sharedService: SharedEvaluacionService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.fechaCreacion = this.obtenerFechaActual();
  }

  ngOnInit(): void {
    this.detectarModoDesdeRuta();
    this.cargarDatosGuardados();
    this.suscribirCambios();
  }

  ngOnDestroy(): void {
    this.guardarDatosEnServicio();
    this.destroy$.next();
    this.destroy$.complete();
    this.limpiarBlobUrls();
  }

  /**
   * Cargar imagen desde el servidor con autenticación JWT
   */
  private async cargarImagenAutenticada(fotoIdBD: number): Promise<string> {
    try {
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
      
      console.log(`Imagen ${fotoIdBD} cargada correctamente`);
      return blobUrl;
      
    } catch (error) {
      console.error(`Error al cargar imagen ${fotoIdBD}:`, error);
      return '/assets/images/placeholder-image.png';
    }
  }

  /**
   * Cargar previews de todas las fotos que vienen de BD
   */
  private async cargarPreviewsFotos(): Promise<void> {
    const fotosConIdBD = this.fotos.filter(f => f.fotoIdBD && !f.preview?.startsWith('data:'));
    
    if (fotosConIdBD.length === 0) return;
    
    console.log(`Cargando ${fotosConIdBD.length} imágenes desde servidor...`);
    
    for (const foto of fotosConIdBD) {
      if (foto.fotoIdBD) {
        foto.preview = await this.cargarImagenAutenticada(foto.fotoIdBD);
      }
    }
    
    console.log('Todas las imágenes cargadas');
  }

  /**
   * Limpiar blob URLs para evitar memory leaks
   */
  private limpiarBlobUrls(): void {
    this.blobUrls.forEach((url) => {
      URL.revokeObjectURL(url);
    });
    this.blobUrls.clear();
    console.log('Blob URLs limpiadas');
  }

  private detectarModoDesdeRuta(): void {
    const modoData = this.route.snapshot.data['modo'] as 'crear' | 'editar' | undefined;
    const idParam = this.route.snapshot.paramMap.get('id');
    
    if (modoData === 'editar' && idParam) {
      this.modoOperacion = 'editar';
      this.evaluacionId = parseInt(idParam, 10);
      console.log('Fase DESPUÉS - Modo EDITAR - ID:', this.evaluacionId);
    } else if (modoData === 'crear') {
      this.modoOperacion = 'crear';
      this.evaluacionId = null;
      console.log('Fase DESPUÉS - Modo CREAR');
    } else {
      const idCompartido = this.sharedService.getEvaluacionId();
      this.modoOperacion = idCompartido ? 'editar' : 'crear';
      this.evaluacionId = idCompartido;
    }
  }

  private suscribirCambios(): void {
    this.sharedService.guardando$
      .pipe(takeUntil(this.destroy$))
      .subscribe(guardando => {
        this.guardando = guardando;
      });
  }

  private cargarDatosGuardados(): void {
    const datosGuardados = this.sharedService.getFaseDespues();
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
      
      console.log('Datos DESPUÉS cargados');
    }
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

    this.sharedService.setFaseDespues(datos);
    this.sharedService.actualizarScore();
  }

  onCampoChange(): void {
    this.guardarDatosEnServicio();
  }

  obtenerFechaActual(): string {
    const hoy = new Date();
    const dia = String(hoy.getDate()).padStart(2, '0');
    const mes = String(hoy.getMonth() + 1).padStart(2, '0');
    const anio = hoy.getFullYear();
    return `${anio}-${mes}-${dia}`;
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      
      const maxSize = 10 * 1024 * 1024;
      if (file.size > maxSize) {
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
      reader.onload = (e: ProgressEvent<FileReader>) => {
        if (e.target?.result) {
          nuevaFoto.preview = e.target.result as string;
        }
      };
      reader.readAsDataURL(file);

      this.fotos.push(nuevaFoto);
      this.onCampoChange();
    }
  }

  /**
   * Manejo de error de imagen con autenticación
   */
  async onImageError(event: Event, foto: FotoLocal): Promise<void> {
    const imgElement = event.target as HTMLImageElement;
    
    console.error('Error al cargar imagen:', {
      fotoId: foto.id,
      fotoIdBD: foto.fotoIdBD
    });
    
    if (foto.fotoIdBD) {
      try {
        console.log('Reintentando con autenticación...');
        const blobUrl = await this.cargarImagenAutenticada(foto.fotoIdBD);
        foto.preview = blobUrl;
        imgElement.src = blobUrl;
      } catch (error) {
        console.error('Falló el reintento:', error);
        imgElement.src = '/assets/images/placeholder-image.png';
      }
    } else {
      imgElement.src = '/assets/images/placeholder-image.png';
    }
  }

  eliminarFoto(id: string | undefined): void {
    if (!id) {
      console.warn('ID de foto inválido');
      return;
    }

    const foto = this.fotos.find(f => f.id === id);
    
    if (!foto) {
      console.warn('Foto no encontrada');
      return;
    }
    
    if (foto.fotoIdBD) {
      if (!confirm('¿Eliminar esta foto? No se puede deshacer')) {
        return;
      }
      
      this.evaluacionService.eliminarFoto(foto.fotoIdBD).subscribe({
        next: () => {
          console.log('Foto eliminada de BD');
          
          if (this.blobUrls.has(foto.fotoIdBD!)) {
            URL.revokeObjectURL(this.blobUrls.get(foto.fotoIdBD!)!);
            this.blobUrls.delete(foto.fotoIdBD!);
          }
          
          this.fotos = this.fotos.filter(f => f.id !== id);
          this.onCampoChange();
        },
        error: (error) => {
          console.error('Error al eliminar foto:', error);
          alert('Error al eliminar la foto de la base de datos');
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

  cambiarFase(fase: 'antes' | 'despues' | 'infogeneral'): void {
    this.guardarDatosEnServicio();
    this.faseActiva = fase;
    
    const rutaBase = this.modoOperacion === 'editar' && this.evaluacionId
      ? `/dashboard/evaluaciones/editar/${this.evaluacionId}`
      : '/dashboard/evaluaciones/nueva';
    
    if (fase === 'infogeneral') {
      this.router.navigate([rutaBase]);
    } else if (fase === 'antes') {
      this.router.navigate([`${rutaBase}/fase-antes`]);
    }
  }

  private generarId(): string {
    return Date.now().toString() + Math.random().toString(36).substring(2, 9);
  }

  guardarEvaluacion(): void {
    if (!this.lugar.trim()) {
      alert('Por favor, ingrese el lugar de la evaluación en esta fase');
      return;
    }

    this.guardarDatosEnServicio();

    const infoGeneral = this.sharedService.getInfoGeneral();
    if (!infoGeneral) {
      alert('No hay información general. Complete primero la pestaña "Información general"');
      const rutaBase = this.modoOperacion === 'editar' && this.evaluacionId
        ? `/dashboard/evaluaciones/editar/${this.evaluacionId}`
        : '/dashboard/evaluaciones/nueva';
      this.router.navigate([rutaBase]);
      return;
    }

    if (!infoGeneral.ordenTrabajoId || !infoGeneral.evaluadorId) {
      alert('Faltan campos obligatorios en Información General:\n\n- Orden de Trabajo\n- Evaluador\n\nComplete estos campos primero.');
      const rutaBase = this.modoOperacion === 'editar' && this.evaluacionId
        ? `/dashboard/evaluaciones/editar/${this.evaluacionId}`
        : '/dashboard/evaluaciones/nueva';
      this.router.navigate([rutaBase]);
      return;
    }

    const validacion = this.sharedService.validar();
    if (!validacion.valido) {
      const erroresFormateados = validacion.errores
        .map((error, index) => `${index + 1}. ${error}`)
        .join('\n');
      alert(`Por favor, corrija los siguientes errores:\n\n${erroresFormateados}`);
      return;
    }

    const dto = this.sharedService.construirDTO();
    if (!dto) {
      alert('Error al preparar los datos. Intente nuevamente.');
      return;
    }

    const mensaje = this.modoOperacion === 'editar' && this.evaluacionId
      ? `¿Actualizar la evaluación #${this.evaluacionId}?`
      : '¿Guardar la evaluación completa?';

    if (!confirm(mensaje)) return;

    this.sharedService.setGuardando(true);

    from(this.evaluacionService.guardarEvaluacionCompleta(dto))
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.sharedService.setGuardando(false))
      )
      .subscribe({
        next: (resultado: EvaluacionResponse) => {
          console.log('Evaluación guardada exitosamente:', resultado);
          
          const mensajeExito = this.modoOperacion === 'editar' && this.evaluacionId
            ? `Evaluación #${resultado.id} actualizada correctamente`
            : `Evaluación creada exitosamente\nID: ${resultado.id}`;
          
          alert(mensajeExito);
          
          if (!this.evaluacionId) {
            this.sharedService.setEvaluacionId(resultado.id);
          }
          
          this.sharedService.limpiar();
          this.router.navigate(['/dashboard/evaluaciones']);
        },
        error: (error: any) => {
          console.error('Error al guardar evaluación:', error);
          const mensaje = error?.error?.message || 'Error desconocido al guardar';
          alert(`Error al guardar la evaluación:\n${mensaje}\n\nIntente nuevamente.`);
        }
      });
  }

  cerrarFormulario(): void {
    const mensaje = this.modoOperacion === 'editar'
      ? '¿Salir sin guardar los cambios?'
      : '¿Cerrar? Los datos no guardados se perderán';
    
    if (confirm(mensaje)) {
      this.sharedService.limpiar();
      this.router.navigate(['/dashboard/evaluaciones']);
    }
  }

  get modoEdicion(): boolean {
    return this.modoOperacion === 'editar';
  }
}