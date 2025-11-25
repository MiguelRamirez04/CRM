import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { EvaluacionService } from '../../../../../../core/services/evaluaciones-registro.service';
import { SharedEvaluacionService } from '../../../../../../core/services/shared-evaluacion.service';
import { FormularioInfoGeneral } from '../../../../../../core/models/evaluaciones-listado.interface';

interface OrdenTrabajo {
  id: string;
  nombre: string;
}

interface Ejecucion {
  id: string;
  nombre: string;
}

interface Cliente {
  id: string;
  nombre: string;
}

interface Evaluador {
  id: string;
  nombre: string;
}

@Component({
  selector: 'app-infogeneral',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './infogeneralregistro.component.html',
  styleUrls: ['./infogeneralregistro.component.css']
})
export class InfogeneralregistroComponent implements OnInit, OnDestroy {
  faseActiva: 'antes' | 'despues' | 'infogeneral' = 'infogeneral';
  private destroy$ = new Subject<void>();
  guardando = false;

  // 🆕 Detectar modo de edición
  modoEdicion = false;
  evaluacionId: number | null = null;

  // Listas para los selectores
  ordenesTrabajo: OrdenTrabajo[] = [];
  ejecuciones: Ejecucion[] = [];
  clientes: Cliente[] = [];
  evaluadores: Evaluador[] = [];

  // Modelo del formulario
  formulario: FormularioInfoGeneral = {
    ordenTrabajoId: '',
    ejecucionId: '',
    clienteId: '',
    evaluadorId: '',
    objetivo: '',
    comentariosGenerales: '',
    scoreCalidad: 0,
    requiereSeguimiento: false,
    notasSeguimiento: ''
  };

  // Estado de las fases
  fases = {
    antes: {
      completada: false,
      titulo: 'Evaluación Antes',
      estatus: 'Sin completar'
    },
    despues: {
      completada: false,
      titulo: 'Evaluación DESPUES',
      estatus: 'Sin completar'
    }
  };

  constructor(
    private evaluacionService: EvaluacionService,
    private sharedService: SharedEvaluacionService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.cargarDatos();
    this.detectarModoEdicion(); // 🆕 Detectar si estamos editando
    this.suscribirACambios();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * 🆕 Detecta si estamos en modo edición o creación
   */
  private detectarModoEdicion(): void {
    this.evaluacionId = this.sharedService.getEvaluacionId();
    this.modoEdicion = this.evaluacionId !== null;

    if (this.modoEdicion) {
      console.log('📝 Modo EDICIÓN activado - Evaluación ID:', this.evaluacionId);
    } else {
      console.log('✨ Modo CREACIÓN - Nueva evaluación');
    }
  }

  /**
   * Suscribe a cambios en el servicio compartido
   */
  private suscribirACambios(): void {
    // Suscribirse a cambios en el score
    this.sharedService.infoGeneral$
      .pipe(takeUntil(this.destroy$))
      .subscribe(info => {
        if (info && info.scoreCalidad !== this.formulario.scoreCalidad) {
          this.formulario.scoreCalidad = info.scoreCalidad;
        }
      });

    // Suscribirse al estado de guardado
    this.sharedService.guardando$
      .pipe(takeUntil(this.destroy$))
      .subscribe(guardando => {
        this.guardando = guardando;
      });

    // Suscribirse a cambios en las fases
    this.sharedService.faseAntes$
      .pipe(takeUntil(this.destroy$))
      .subscribe(fase => {
        this.fases.antes.completada = fase !== null;
        this.fases.antes.estatus = fase ? 'Completada' : 'Sin completar';
      });

    this.sharedService.faseDespues$
      .pipe(takeUntil(this.destroy$))
      .subscribe(fase => {
        this.fases.despues.completada = fase !== null;
        this.fases.despues.estatus = fase ? 'Completada' : 'Sin completar';
      });

    // 🆕 Suscribirse a cambios en evaluacionId
    this.sharedService.evaluacionId$
      .pipe(takeUntil(this.destroy$))
      .subscribe(id => {
        if (id !== this.evaluacionId) {
          this.evaluacionId = id;
          this.modoEdicion = id !== null;
        }
      });
  }

  /**
   * Carga los datos iniciales (catálogos)
   */
  cargarDatos(): void {
    // TODO: Reemplazar con llamadas reales a tus servicios de catálogos
    this.ordenesTrabajo = [
      { id: '1', nombre: 'Orden 001 - Instalación' },
      { id: '2', nombre: 'Orden 002 - Mantenimiento' },
      { id: '3', nombre: 'Orden 003 - Reparación' }
    ];

    this.ejecuciones = [
      { id: '1', nombre: 'Ejecución 001' },
      { id: '2', nombre: 'Ejecución 002' }
    ];

    this.clientes = [
      { id: '1', nombre: 'Cliente A - Empresa X' },
      { id: '2', nombre: 'Cliente B - Empresa Y' }
    ];

    this.evaluadores = [
      { id: '1', nombre: 'Juan Pérez' },
      { id: '2', nombre: 'María González' },
      { id: '3', nombre: 'Carlos Rodríguez' }
    ];

    // Cargar datos guardados si existen
    const infoGuardada = this.sharedService.getInfoGeneral();
    if (infoGuardada) {
      this.formulario = { ...infoGuardada };
      console.log('✅ Datos cargados del SharedService:', this.formulario);
    }
  }

  /**
   * Guarda los datos en el servicio compartido cuando cambian
   */
  onFormularioChange(): void {
    this.sharedService.setInfoGeneral({ ...this.formulario });
  }

  alternarSeguimiento(): void {
    this.formulario.requiereSeguimiento = !this.formulario.requiereSeguimiento;
    this.onFormularioChange();
  }

  seleccionarFase(tipoFase: 'antes' | 'despues'): void {
    this.cambiarFase(tipoFase);
  }

  cambiarFase(fase: 'antes' | 'despues' | 'infogeneral'): void {
    // Guardar datos actuales antes de cambiar
    this.onFormularioChange();
    
    this.faseActiva = fase;
    
    // Navegar a la ruta correspondiente
    if (fase === 'antes') {
      this.router.navigate(['/dashboard/evaluaciones/registro/fase-antes']);
    } else if (fase === 'despues') {
      this.router.navigate(['/dashboard/evaluaciones/registro/fase-despues']);
    }
  }

  get estatusSeguimiento(): string {
    return this.formulario.requiereSeguimiento ? 'Requiere seguimiento' : 'Sin seguimiento';
  }

  /**
   * 🆕 Título dinámico según el modo
   */
  get tituloFormulario(): string {
    return this.modoEdicion 
      ? `Editar Evaluación #${this.evaluacionId}` 
      : 'Nueva evaluación';
  }

  /**
   * Guarda la evaluación completa
   */
  async guardarEvaluacion(): Promise<void> {
    // Validar campos obligatorios
    if (!this.formulario.ordenTrabajoId || !this.formulario.evaluadorId) {
      alert('Por favor complete los campos obligatorios: Orden de Trabajo y Evaluador');
      return;
    }

    // Guardar datos actuales en el servicio compartido
    this.onFormularioChange();

    // Actualizar score antes de guardar
    this.sharedService.actualizarScoreEnInfoGeneral();

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

    // 🆕 Mensaje personalizado según el modo
    const mensajeConfirmacion = this.modoEdicion
      ? '¿Desea guardar los cambios realizados en la evaluación?'
      : '¿Desea guardar la evaluación completa?';

    const confirmar = confirm(mensajeConfirmacion);
    if (!confirmar) {
      return;
    }

    try {
      this.sharedService.setGuardando(true);
      
      // 🆕 Llamar al servicio para guardar (funciona igual para crear y actualizar)
      const resultado = await this.evaluacionService.guardarEvaluacionCompleta(evaluacionCompleta);
      
      console.log('✅ Evaluación guardada exitosamente:', resultado);
      
      const mensajeExito = this.modoEdicion
        ? `Evaluación #${resultado.id} actualizada exitosamente`
        : `Evaluación guardada exitosamente con ID: ${resultado.id}`;
      
      alert(mensajeExito);
      
      // Actualizar el ID de la evaluación en el servicio compartido
      this.sharedService.setEvaluacionId(resultado.id);
      
      // Cambiar a modo edición si era creación
      if (!this.modoEdicion) {
        this.modoEdicion = true;
        this.evaluacionId = resultado.id;
      }
      
      // Opcional: Navegar al listado después de guardar
      const irAListado = confirm('¿Desea volver al listado de evaluaciones?');
      if (irAListado) {
        this.sharedService.limpiar();
        this.router.navigate(['/dashboard/evaluaciones']);
      }
      
    } catch (error) {
      console.error('❌ Error al guardar evaluación:', error);
      alert('Error al guardar la evaluación. Por favor intente nuevamente.');
    } finally {
      this.sharedService.setGuardando(false);
    }
  }

  /**
   * Cierra el formulario
   */
  cerrarFormulario(): void {
    const mensaje = this.modoEdicion
      ? '¿Está seguro de salir sin guardar los cambios?'
      : '¿Está seguro de cerrar? Los datos no guardados se perderán.';
    
    const confirmar = confirm(mensaje);
    if (confirmar) {
      this.sharedService.limpiar();
      this.router.navigate(['/dashboard/evaluaciones']);
    }
  }
}