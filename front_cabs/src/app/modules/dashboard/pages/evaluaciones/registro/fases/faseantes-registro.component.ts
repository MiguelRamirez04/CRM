import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { EvaluacionService } from '../../../../../../core/services/evaluaciones-registro.service';
import { SharedEvaluacionService } from '../../../../../../core/services/shared-evaluacion.service';
import { Foto } from '../../../../../../core/models/evaluaciones-listado.interface';

@Component({
  selector: 'app-fases',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './fases.component.html',
  styleUrls: ['./fases.component.css']
})
export class FaseAntesRegistroComponent implements OnInit, OnDestroy {
  lugar: string = '';
  fechaCreacion: string = '';
  scoreFase: number = 0;
  descripcion: string = '';
  sugerencias: string = '';
  notaGeneral: string = '';
  fotos: Foto[] = [];
  faseActiva: 'antes' | 'despues' | 'infogeneral' = 'antes';
  tituloFase: string = 'ANTES';
  guardando = false;
  detalleId?: number;

  private destroy$ = new Subject<void>();

  constructor(
    private evaluacionService: EvaluacionService,
    private sharedService: SharedEvaluacionService,
    private router: Router
  ) {
    this.fechaCreacion = this.obtenerFechaActual();
  }

  ngOnInit(): void {
    this.cargarDatosGuardados();
    this.suscribirACambios();
  }

  ngOnDestroy(): void {
    // Guardar datos antes de destruir el componente
    this.guardarDatosEnServicio();
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Suscribe a cambios en el servicio compartido
   */
  private suscribirACambios(): void {
    this.sharedService.guardando$
      .pipe(takeUntil(this.destroy$))
      .subscribe(guardando => {
        this.guardando = guardando;
      });

    this.sharedService.faseAntesDetalleId$
      .pipe(takeUntil(this.destroy$))
      .subscribe(detalleId => {
        if (detalleId) {
          this.detalleId = detalleId;
        }
      });
  }

  /**
   * Carga datos previamente guardados en el servicio
   */
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
    }
  }

  /**
   * Guarda los datos actuales en el servicio compartido
   */
  private guardarDatosEnServicio(): void {
    this.sharedService.setFaseAntes({
      lugar: this.lugar,
      fechaCreacion: this.fechaCreacion,
      scoreFase: this.scoreFase,
      descripcion: this.descripcion,
      sugerencias: this.sugerencias,
      notaGeneral: this.notaGeneral,
      fotos: this.fotos,
      detalleId: this.detalleId
    });

    // Actualizar el score total
    this.sharedService.actualizarScoreEnInfoGeneral();
  }

  /**
   * Se llama cuando cualquier campo cambia
   */
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
      
      // Validar tamaño (máximo 10MB)
      const maxSize = 10 * 1024 * 1024; // 10MB
      if (file.size > maxSize) {
        alert('El archivo es demasiado grande. Tamaño máximo: 10MB');
        return;
      }

      // Validar tipo
      const allowedTypes = ['image/jpeg', 'image/png', 'image/jpg', 'image/webp', 'image/gif'];
      if (!allowedTypes.includes(file.type)) {
        alert('Tipo de archivo no permitido. Use: JPG, PNG, WebP o GIF');
        return;
      }

      const nuevaFoto: Foto = {
        id: this.generarId(),
        tipo: 'General',
        fecha: this.obtenerFechaActual(),
        descripcion: '',
        archivo: file
      };

      // Crear preview de la imagen
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

  eliminarFoto(id: string | undefined): void {
    // Validar que el ID exista
    if (!id) {
      console.warn('Intentando eliminar foto sin ID válido');
      return;
    }

    const foto = this.fotos.find(f => f.id === id);
    
    if (!foto) {
      console.warn('Foto no encontrada');
      return;
    }
    
    if (foto.fotoIdBD) {
      // Si la foto ya está en la base de datos, confirmar eliminación
      const confirmar = confirm('¿Está seguro de eliminar esta foto? Esta acción no se puede deshacer.');
      if (!confirmar) {
        return;
      }
      
      // Eliminar de la base de datos
      this.evaluacionService.deleteFoto(foto.fotoIdBD).subscribe({
        next: () => {
          console.log('Foto eliminada de la base de datos');
          this.fotos = this.fotos.filter(f => f.id !== id);
          this.onCampoChange();
        },
        error: (error) => {
          console.error('Error al eliminar foto:', error);
          alert('Error al eliminar la foto de la base de datos');
        }
      });
    } else {
      // Foto solo local, eliminar directamente
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
    // Guardar datos antes de cambiar
    this.guardarDatosEnServicio();
    
    this.faseActiva = fase;
    
    // Navegar
    if (fase === 'infogeneral') {
      this.router.navigate(['/dashboard/evaluaciones/registro']);
    } else if (fase === 'despues') {
      this.router.navigate(['/dashboard/evaluaciones/registro/fase-despues']);
    }
  }

  private generarId(): string {
    return Date.now().toString() + Math.random().toString(36).substring(2, 9);
  }

  /**
   * Guarda la evaluación completa
   */
  async guardarEvaluacion(): Promise<void> {
    // Validar campos obligatorios
    if (!this.lugar.trim()) {
      alert('Por favor ingrese el lugar de la evaluación');
      return;
    }

    // Guardar datos actuales
    this.guardarDatosEnServicio();

    // Validar datos completos
    const validacion = this.sharedService.validarDatosCompletos();
    if (!validacion.valido) {
      const mensaje = 'Errores de validación:\n' + validacion.errores.join('\n');
      alert(mensaje);
      return;
    }

    // Construir DTO completo
    const evaluacionCompleta = this.sharedService.construirEvaluacionCompleta();
    if (!evaluacionCompleta) {
      alert('Error al preparar los datos para guardar');
      return;
    }

    // Confirmar
    const confirmar = confirm('¿Desea guardar la evaluación completa?');
    if (!confirmar) {
      return;
    }

    try {
      this.sharedService.setGuardando(true);
      
      const resultado = await this.evaluacionService.guardarEvaluacionCompleta(evaluacionCompleta);
      
      console.log('Evaluación guardada exitosamente:', resultado);
      alert(`Evaluación guardada exitosamente con ID: ${resultado.id}`);
      
      this.sharedService.setEvaluacionId(resultado.id);
      
    } catch (error) {
      console.error('Error al guardar evaluación:', error);
      alert('Error al guardar la evaluación. Por favor intente nuevamente.');
    } finally {
      this.sharedService.setGuardando(false);
    }
  }

  /**
   * Cierra el formulario
   */
  cerrarFormulario(): void {
    const confirmar = confirm('¿Está seguro de cerrar? Los datos no guardados se perderán.');
    if (confirmar) {
      this.sharedService.limpiar();
      this.router.navigate(['/dashboard/evaluaciones']);
    }
  }
}