import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

interface ProductoServicio {
  nombre: string;
  descripcion: string;
  cantidad: number;
  precioUnitario: number;
  descuento: number;
  subtotal: number;
}

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

  cotizacionForm: FormGroup;
  fechaContactoHeader = this.fb.control(this.getFechaActual());
  tipoClienteSeleccionado: 'particular' | 'empresa' = 'particular';
  
  subtotal = 0;
  iva = 0;
  total = 0;
  
  fechaActualizacion = '29/09/2025';
  responsableActualizacion = 'Javier';

  constructor() {
    this.cotizacionForm = this.fb.group({
      nombreCotizacion: ['', Validators.required],
      responsable: [''],
      tipoCliente: ['particular'],
      nombreEmpresa: [''],
      nombreCliente: [''],
      medioContacto: [''],
      fechaContacto: [this.getFechaActual()],
      validoHasta: [''],
      observaciones: [''],
      productos: this.fb.array([]),
      comision: [0]
    });
  }

  ngOnInit(): void {
    // Escuchar cambios en comision
    this.cotizacionForm.get('comision')?.valueChanges.subscribe(() => {
      this.calcularTotales();
    });
  }

  get productos(): FormArray {
    return this.cotizacionForm.get('productos') as FormArray;
  }

  getFechaActual(): string {
    const today = new Date();
    const year = today.getFullYear();
    const month = String(today.getMonth() + 1).padStart(2, '0');
    const day = String(today.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  setTipoCliente(tipo: 'particular' | 'empresa'): void {
    this.tipoClienteSeleccionado = tipo;
    this.cotizacionForm.patchValue({ tipoCliente: tipo });
  }

  agregarProducto(): void {
    const productoGroup = this.fb.group({
      nombre: [''],
      descripcion: [''],
      cantidad: [0],
      precioUnitario: [0],
      descuento: [0],
      subtotal: [0]
    });

    this.productos.push(productoGroup);
  }

  eliminarProducto(index: number): void {
    this.productos.removeAt(index);
    this.calcularTotales();
  }

  calcularSubtotal(index: number): void {
    const producto = this.productos.at(index);
    const cantidad = producto.get('cantidad')?.value || 0;
    const precioUnitario = producto.get('precioUnitario')?.value || 0;
    const descuento = producto.get('descuento')?.value || 0;

    const subtotal = cantidad * precioUnitario * (1 - descuento / 100);
    producto.patchValue({ subtotal }, { emitEvent: false });
    
    this.calcularTotales();
  }

  calcularTotales(): void {
    this.subtotal = this.productos.controls.reduce((acc, producto) => {
      return acc + (producto.get('subtotal')?.value || 0);
    }, 0);

    this.iva = this.subtotal * 0.16;
    this.total = this.subtotal + this.iva;
  }

  onGuardar(): void {
    if (this.cotizacionForm.valid) {
      console.log('Guardando cotización:', this.cotizacionForm.value);
      alert('Cotización guardada exitosamente');
    } else {
      Object.keys(this.cotizacionForm.controls).forEach(key => {
        this.cotizacionForm.get(key)?.markAsTouched();
      });
      alert('Por favor complete los campos requeridos');
    }
  }

  onGuardarYEnviar(): void {
    if (this.cotizacionForm.valid) {
      console.log('Guardando y enviando cotización:', this.cotizacionForm.value);
      alert('Cotización guardada y enviada exitosamente');
    } else {
      alert('Por favor complete los campos requeridos');
    }
  }

  onGenerarPDF(): void {
    console.log('Generando PDF de la cotización');
    alert('Función de PDF en desarrollo');
  }
}