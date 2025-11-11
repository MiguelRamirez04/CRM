// =====================================================================================
// COMPONENTE CLIENTES COMPLETOS - clientes-completos.component.ts
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Página dedicada para consultar clientes de la BD legacy (ClientesCompletos).
// Incluye tabla paginada, búsquedas por nombre/RFC y filtros avanzados.
//
// ACCESO: Solo roles Recepcion y Administracion
//
// =====================================================================================

import { Component, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ClientesCompletosService } from '../../services/clientes-completos.service';
import { HeaderComponent } from '../../../../layout/header/header.component';
import { ClienteCompleto, PagedResponse } from '../../../../core/models/cliente-completo.interface';

@Component({
  selector: 'app-clientes-completos',
  standalone: true,
  imports: [CommonModule, FormsModule, HeaderComponent],
  templateUrl: './clientes-completos.component.html',
  styleUrls: ['./clientes-completos.component.css']
})
export class ClientesCompletosComponent implements OnInit {
  private clientesService = inject(ClientesCompletosService);

  // Señales para estado reactivo
  clientes = signal<ClienteCompleto[]>([]);
  cargando = signal<boolean>(false);
  error = signal<string | null>(null);
  paginaActual = signal<number>(1);
  totalPaginas = signal<number>(0);
  totalRegistros = signal<number>(0);

  // Filtros de búsqueda
  busquedaNombre = signal<string>('');
  busquedaRfc = signal<string>('');

  // Modo de búsqueda actual
  modoBusqueda = signal<'todos' | 'nombre' | 'rfc'>('todos');

  ngOnInit(): void {
    this.cargarClientes();
  }

  // Cargar todos los clientes (paginados)
  cargarClientes(pagina: number = 1): void {
    this.cargando.set(true);
    this.error.set(null);
    this.modoBusqueda.set('todos');

    this.clientesService.getClientesPaginados(pagina, 20).subscribe({
      next: (response: PagedResponse<ClienteCompleto>) => {
        this.clientes.set(response.data);
        this.paginaActual.set(response.paginaActual);
        this.totalPaginas.set(response.totalPaginas);
        this.totalRegistros.set(response.totalRegistros);
        this.cargando.set(false);
      },
      error: (err) => {
        this.error.set(err.message);
        this.cargando.set(false);
      }
    });
  }

  // Buscar por nombre
  buscarPorNombre(): void {
    const nombre = this.busquedaNombre().trim();
    if (!nombre || nombre.length < 2) {
      this.error.set('Ingrese al menos 2 caracteres para buscar por nombre');
      return;
    }

    this.cargando.set(true);
    this.error.set(null);
    this.modoBusqueda.set('nombre');

    this.clientesService.buscarPorNombre(nombre).subscribe({
      next: (response: PagedResponse<ClienteCompleto>) => {
        this.clientes.set(response.data);
        this.paginaActual.set(response.paginaActual);
        this.totalPaginas.set(response.totalPaginas);
        this.totalRegistros.set(response.totalRegistros);
        this.cargando.set(false);
      },
      error: (err) => {
        this.error.set(err.message);
        this.cargando.set(false);
      }
    });
  }

  // Buscar por RFC
  buscarPorRfc(): void {
    const rfc = this.busquedaRfc().trim();
    if (!rfc) {
      this.error.set('Ingrese un RFC válido');
      return;
    }

    this.cargando.set(true);
    this.error.set(null);
    this.modoBusqueda.set('rfc');

    this.clientesService.buscarPorRfc(rfc).subscribe({
      next: (response: PagedResponse<ClienteCompleto>) => {
        this.clientes.set(response.data);
        this.paginaActual.set(response.paginaActual);
        this.totalPaginas.set(response.totalPaginas);
        this.totalRegistros.set(response.totalRegistros);
        this.cargando.set(false);
      },
      error: (err) => {
        this.error.set(err.message);
        this.cargando.set(false);
      }
    });
  }



  // Limpiar búsquedas y volver a vista general
  limpiarBusquedas(): void {
    this.busquedaNombre.set('');
    this.busquedaRfc.set('');
    this.cargarClientes();
  }

  // Navegación de páginas
  paginaAnterior(): void {
    if (this.paginaActual() > 1) {
      this.navegarAPagina(this.paginaActual() - 1);
    }
  }

  paginaSiguiente(): void {
    if (this.paginaActual() < this.totalPaginas()) {
      this.navegarAPagina(this.paginaActual() + 1);
    }
  }

  // Navegar a página específica según el modo actual
  navegarAPagina(pagina: number): void {
    const modo = this.modoBusqueda();
    switch (modo) {
      case 'nombre':
        this.buscarPorNombrePagina(pagina);
        break;
      case 'rfc':
        this.buscarPorRfcPagina(pagina);
        break;
      default:
        this.cargarClientes(pagina);
    }
  }

  // Métodos auxiliares para paginación por modo
  private buscarPorNombrePagina(pagina: number): void {
    const nombre = this.busquedaNombre().trim();
    if (nombre) {
      this.cargando.set(true);
      this.clientesService.buscarPorNombre(nombre, pagina, 20).subscribe({
        next: (response: PagedResponse<ClienteCompleto>) => {
          this.clientes.set(response.data);
          this.paginaActual.set(response.paginaActual);
          this.totalPaginas.set(response.totalPaginas);
          this.totalRegistros.set(response.totalRegistros);
          this.cargando.set(false);
        },
        error: (err) => {
          this.error.set(err.message);
          this.cargando.set(false);
        }
      });
    }
  }

  private buscarPorRfcPagina(pagina: number): void {
    const rfc = this.busquedaRfc().trim();
    if (rfc) {
      this.cargando.set(true);
      this.clientesService.buscarPorRfc(rfc, pagina, 20).subscribe({
        next: (response: PagedResponse<ClienteCompleto>) => {
          this.clientes.set(response.data);
          this.paginaActual.set(response.paginaActual);
          this.totalPaginas.set(response.totalPaginas);
          this.totalRegistros.set(response.totalRegistros);
          this.cargando.set(false);
        },
        error: (err) => {
          this.error.set(err.message);
          this.cargando.set(false);
        }
      });
    }
  }





  // Formatear fecha para display (mantenido para compatibilidad)
  formatearFecha(fecha: string): string {
    if (!fecha) return 'N/A';
    return new Date(fecha).toLocaleDateString('es-MX');
  }

  // TrackBy function para optimizar el rendering de la tabla
  trackByClienteId(index: number, cliente: ClienteCompleto): number {
    return cliente.clienteId;
  }
}