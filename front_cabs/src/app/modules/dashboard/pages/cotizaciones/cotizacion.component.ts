import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CotizacionService } from '../../../../core/services/cotizacion.service';
import { EstadoCotizacion } from '../../../../core/enums/estado-cotizacion.enum';
import { CotizacionCreateRequest } from '../../../../core/models/cotizacion.interface';
import { SecureAuthService } from '../../../../core/services/secure-auth.service';

@Component({
  selector: 'app-cotizaciones',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './cotizacion.component.html',
  styleUrls: ['./cotizacion.component.css']
})
export class CotizacionComponent implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private cotizacionService = inject(CotizacionService);
  private authService = inject(SecureAuthService);

  cotizacionForm: FormGroup;
  
  // Estados disponibles para el selector
  estadosDisponibles = Object.values(EstadoCotizacion);
  
  // Cálculos en tiempo real
  totalCalculado = 0;
  totalFinal = 0;
  
  // Control de UI
  guardando = false;
  submitted = false;
  mensajeExito = '';
  mensajeError = '';
  mostrarCapacitacion = false;

  constructor() {
    this.cotizacionForm = this.fb.group({
      // Información del Cliente
      cliente: ['', [Validators.required, Validators.maxLength(255)]],
      rfc: ['', [
        Validators.required, 
        Validators.minLength(12), 
        Validators.maxLength(13),
        Validators.pattern(/^[A-ZÑ&]{3,4}\d{6}[A-Z0-9]{3}$/)
      ]],
      telefono: [null, [
        Validators.min(10000), 
        Validators.max(999999999999999)
      ]],
      correo: ['', [Validators.email, Validators.maxLength(150)]],
      
      // Datos de la Cotización
      folio: [''], // Opcional - Se genera automáticamente en el backend
      estado: [EstadoCotizacion.NUEVA, Validators.required],
      validezDias: [30, [Validators.required, Validators.min(1), Validators.max(365)]],
      
      // Detalles Adicionales
      descripcionServicio: ['', [Validators.required, Validators.maxLength(1000)]],
      observaciones: ['', Validators.maxLength(500)],
      
      // Capacitación (campos opcionales)
      horasCapacitacion: [null, [Validators.min(0), Validators.max(999)]],
      paquetesCapacitacion: [null, [Validators.min(0), Validators.max(999)]],
      costoCapacitacion: [null, [Validators.min(0)]],
      
      // Información Financiera
      subtotal: [0, [Validators.required, Validators.min(0)]],
      impuestosTotal: [0, [Validators.required, Validators.min(0)]],
      descuento: [0, [Validators.min(0)]]
    });
  }

  ngOnInit(): void {
    // Escuchar cambios en subtotal, impuestos y descuento para calcular totales
    this.cotizacionForm.get('subtotal')?.valueChanges.subscribe(() => this.calcularTotales());
    this.cotizacionForm.get('impuestosTotal')?.valueChanges.subscribe(() => this.calcularTotales());
    this.cotizacionForm.get('descuento')?.valueChanges.subscribe(() => this.calcularTotales());
    
    // Calcular inicial
    this.calcularTotales();
  }

  /**
   * Toggle para mostrar/ocultar panel de capacitación
   */
  toggleCapacitacion(): void {
    this.mostrarCapacitacion = !this.mostrarCapacitacion;
  }

  /**
   * Calcula Total y Total Final en tiempo real
   * Total = Subtotal + Impuestos (simula columna PERSISTED de BD)
   * Total Final = Total - Descuento
   */
  calcularTotales(): void {
    const subtotal = Number(this.cotizacionForm.get('subtotal')?.value) || 0;
    const impuestos = Number(this.cotizacionForm.get('impuestosTotal')?.value) || 0;
    const descuento = Number(this.cotizacionForm.get('descuento')?.value) || 0;
    
    // Simula cálculo de BD
    this.totalCalculado = subtotal + impuestos;
    this.totalFinal = this.totalCalculado - descuento;
    
    console.log('📊 Totales calculados:', {
      subtotal,
      impuestos,
      descuento,
      totalCalculado: this.totalCalculado,
      totalFinal: this.totalFinal
    });
  }

  /**
   * Calcula automáticamente impuestos (16% IVA sobre subtotal)
   */
  calcularImpuestos(): void {
    const subtotal = Number(this.cotizacionForm.get('subtotal')?.value) || 0;
    const iva = Math.round(subtotal * 0.16 * 100) / 100; // Redondear a 2 decimales
    
    console.log('🧮 Calculando IVA:', {
      subtotal,
      porcentaje: '16%',
      ivaCalculado: iva
    });
    
    this.cotizacionForm.patchValue({ impuestosTotal: iva });
    this.calcularTotales();
  }

  /**
   * Valida RFC usando el servicio
   */
  validarRFC(): boolean {
    const rfc = this.cotizacionForm.get('rfc')?.value;
    if (!rfc) return false;
    return this.cotizacionService.validarRFC(rfc);
  }

  /**
   * Valida correo usando el servicio
   */
  validarCorreo(): boolean {
    const correo = this.cotizacionForm.get('correo')?.value;
    if (!correo) return true; // Correo es opcional
    return this.cotizacionService.validarCorreo(correo);
  }

  /**
   * Valida teléfono usando el servicio
   */
  validarTelefono(): boolean {
    const telefono = this.cotizacionForm.get('telefono')?.value;
    if (!telefono) return true; // Teléfono es opcional
    return this.cotizacionService.validarTelefono(telefono);
  }

  /**
   * Formatea teléfono para display
   */
  formatearTelefono(telefono: number): string {
    return this.cotizacionService.formatearTelefono(telefono);
  }

  /**
   * Obtiene el label en español del estado
   */
  obtenerLabelEstado(estado: EstadoCotizacion): string {
    return this.cotizacionService.obtenerLabelEstado(estado);
  }

  /**
   * Obtiene clase CSS para el estado
   */
  obtenerClaseEstado(estado: EstadoCotizacion): string {
    return this.cotizacionService.obtenerClaseEstado(estado);
  }

  /**
   * Método auxiliar para debugging - obtiene todos los errores del formulario
   */
  private obtenerErroresFormulario(): any {
    const errores: any = {};
    Object.keys(this.cotizacionForm.controls).forEach(key => {
      const control = this.cotizacionForm.get(key);
      if (control && control.errors) {
        errores[key] = control.errors;
      }
    });
    return errores;
  }

  /**
   * Método principal para crear cotización (POST)
   */
  async onGuardar(): Promise<void> {
    console.log('🟢🟢🟢 onGuardar() EJECUTADO - INICIO DEL MÉTODO');
    console.log('Estado guardando:', this.guardando);
    console.log('Estado submitted:', this.submitted);
    
    // Prevenir múltiples submissions
    if (this.guardando || this.submitted) {
      console.log('⚠️ Formulario ya enviado o en proceso - Ignorando submit');
      return;
    }

    console.log('🔵 onGuardar() ejecutado - Iniciando validación...');
    console.log('📋 Valores del formulario:', this.cotizacionForm.value);
    console.log('✅ Formulario válido?', this.cotizacionForm.valid);
    console.log('❌ Formulario inválido?', this.cotizacionForm.invalid);
    
    // Marcar todos los campos como touched para mostrar errores
    Object.keys(this.cotizacionForm.controls).forEach(key => {
      this.cotizacionForm.get(key)?.markAsTouched();
    });

    // Validar formulario
    if (this.cotizacionForm.invalid) {
      console.log('❌ Formulario inválido - Errores:', this.obtenerErroresFormulario());
      this.mensajeError = 'Por favor complete todos los campos requeridos correctamente';
      setTimeout(() => this.mensajeError = '', 5000);
      return;
    }
    
    console.log('✅ Formulario válido - Continuando con validaciones...');

    // Validaciones adicionales
    const rfc = this.cotizacionForm.get('rfc')?.value;
    if (rfc && !this.validarRFC()) {
      console.log('❌ RFC inválido:', rfc);
      this.mensajeError = 'El RFC no tiene un formato válido';
      setTimeout(() => this.mensajeError = '', 5000);
      return;
    }

    const correo = this.cotizacionForm.get('correo')?.value;
    if (correo && !this.validarCorreo()) {
      console.log('❌ Correo inválido:', correo);
      this.mensajeError = 'El correo electrónico no tiene un formato válido';
      setTimeout(() => this.mensajeError = '', 5000);
      return;
    }

    const telefono = this.cotizacionForm.get('telefono')?.value;
    if (telefono && !this.validarTelefono()) {
      console.log('❌ Teléfono inválido:', telefono);
      this.mensajeError = 'El teléfono debe tener entre 5 y 15 dígitos';
      setTimeout(() => this.mensajeError = '', 5000);
      return;
    }

    // Validar que se haya calculado el IVA correctamente
    const subtotal = this.cotizacionForm.get('subtotal')?.value || 0;
    const impuestosTotal = this.cotizacionForm.get('impuestosTotal')?.value || 0;
    const ivaEsperado = subtotal * 0.16;
    
    console.log('💰 Validación IVA:', { subtotal, impuestosTotal, ivaEsperado, diferencia: Math.abs(impuestosTotal - ivaEsperado) });
    
    if (subtotal > 0 && Math.abs(impuestosTotal - ivaEsperado) > 0.01) {
      console.log('⚠️ IVA incorrecto - debe calcularse con el botón Calc');
      this.mensajeError = '⚠️ Por favor presione el botón "Calc" para calcular correctamente el IVA (16%) antes de crear la cotización';
      setTimeout(() => this.mensajeError = '', 6000);
      return;
    }

    // Preparar datos para enviar (ordenId es opcional)
    const cotizacionData: CotizacionCreateRequest = {
      ordenId: null, // Sin orden de trabajo asociada
      subtotal: this.cotizacionForm.get('subtotal')?.value,
      impuestosTotal: this.cotizacionForm.get('impuestosTotal')?.value,
      descuento: this.cotizacionForm.get('descuento')?.value || null,
      estado: this.cotizacionForm.get('estado')?.value,
      observaciones: this.cotizacionForm.get('observaciones')?.value || null,
      cliente: this.cotizacionForm.get('cliente')?.value,
      rfc: this.cotizacionForm.get('rfc')?.value,
      folio: this.cotizacionForm.get('folio')?.value || null,
      descripcionServicio: this.cotizacionForm.get('descripcionServicio')?.value,
      validezDias: this.cotizacionForm.get('validezDias')?.value,
      horasCapacitacion: this.cotizacionForm.get('horasCapacitacion')?.value || null,
      paquetesCapacitacion: this.cotizacionForm.get('paquetesCapacitacion')?.value || null,
      costoCapacitacion: this.cotizacionForm.get('costoCapacitacion')?.value || null,
      telefono: this.cotizacionForm.get('telefono')?.value || null,
      correo: this.cotizacionForm.get('correo')?.value || null
    };

    // Enviar al backend
    console.log('🚀 Enviando cotización al backend...');
    console.log('📦 Datos a enviar:', cotizacionData);
    
    this.guardando = true;
    this.mensajeError = '';
    this.mensajeExito = '';

    try {
      // Asegurar que tenemos el token CSRF antes de hacer el POST
      console.log('🔒 Verificando token CSRF...');
      await this.authService.obtenerCsrfToken().toPromise();
      
      console.log('📡 Llamando a API POST /api/Cotizaciones...');
      const resultado = await this.cotizacionService.crear(cotizacionData).toPromise();
      
      console.log('✅ Respuesta del backend:', resultado);
      
      this.mensajeExito = `✅ Cotización creada exitosamente!
      ID: ${resultado?.id}
      Cliente: ${resultado?.cliente}
      Total: $${resultado?.total?.toFixed(2)}
      Folio: ${resultado?.folio}`;
      
      console.log('✅ Cotización creada exitosamente:', resultado);
      
      // Marcar como enviado exitosamente
      this.submitted = true;
      
      // Redireccionar al listado después de mostrar el mensaje de éxito
      setTimeout(() => {
        this.router.navigate(['/dashboard/cotizaciones/vista']);
      }, 3000);
      
    } catch (error: any) {
      console.error(' ERROR al crear cotización:', error);
      console.log(' Status:', error?.status);
      console.error(' Error completo:', JSON.stringify(error, null, 2));
      
      const errorMsg = error?.error?.errors 
        ? Object.values(error.error.errors).flat().join(', ')
        : error?.error?.message || error?.message || 'No se pudo crear la cotización';
      this.mensajeError = ` Error: ${errorMsg}`;
      setTimeout(() => this.mensajeError = '', 7000);

      // Reset submitted state on error
      this.submitted = false;
    } finally {
      this.guardando = false;
      console.log(' onGuardar() finalizado');
    }
  }

  /**
   * Limpia el formulario
   */
  onLimpiar(): void {
    // No permitir limpiar si se está guardando
    if (this.guardando) return;

    this.cotizacionForm.reset({
      estado: EstadoCotizacion.NUEVA,
      validezDias: 30,
      subtotal: 0,
      impuestosTotal: 0,
      descuento: 0
    });
    
    this.mostrarCapacitacion = false;
    this.calcularTotales();
    this.mensajeExito = '';
    this.mensajeError = '';
    this.submitted = false; // Reset el estado de submitted al limpiar
  }

  /**
   * Método de prueba para verificar que los eventos click funcionan
   */
  testMethod(): void {
    console.log('🧪 TEST METHOD EJECUTADO');
    console.log('Formulario válido:', this.cotizacionForm.valid);
    console.log('Valores del formulario:', this.cotizacionForm.value);
    console.log('Errores del formulario:', this.obtenerErroresFormulario());
    alert('Test ejecutado - Revisa la consola (F12)');
  }
}
