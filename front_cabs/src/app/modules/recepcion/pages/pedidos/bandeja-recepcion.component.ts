import { Component, OnInit, inject } from '@angular/core';
import { RecepcionService } from '../../../../core/services/recepcion.service';
import { RecepcionTicket } from '../../../../core/models/recepcion.model';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

// 👇 1. IMPORTAR EL SERVICIO DE DIÁLOGO Y LOS COMPONENTES
import { DialogService } from '../../services/dialog.service';
import { NuevaOrdenClienteNuevoComponent } from '../../components/dialogs/nueva-orden-cliente-nuevo/nueva-orden-cliente-nuevo.component';
import { NuevaOrdenClienteLegacyComponent } from '../../components/dialogs/nueva-orden-cliente-legacy/nueva-orden-cliente-legacy.component';

@Component({
  selector: 'app-bandeja-recepcion',
  standalone: true, 
  imports: [
    CommonModule, 
    FormsModule,
    // 👇 2. IMPORTAR LOS COMPONENTES DE DIÁLOGO (si son standalone)
    // Si no son standalone, no necesitas importarlos aquí, solo en el DialogService.
    // Asumiremos que DialogService ya los maneja.
  ], 
  templateUrl: './bandeja-recepcion.component.html',
  styleUrls: ['./bandeja-recepcion.component.css']
})
export class BandejaRecepcionComponent implements OnInit {
  
  private recepcionService = inject(RecepcionService);
  private dialogService = inject(DialogService); // 3. INYECTAR EL SERVICIO
  
  public tickets: RecepcionTicket[] = [];
  public isLoading = false;
  
  // Para paginación
  public totalItems = 0; 
  public currentPage = 1;
  public pageSize = 10; 
  
  // Para filtros
  public filtroEstado = ''; 

  ngOnInit(): void {
    this.loadTickets();
  }

  loadTickets(): void {
    this.isLoading = true;
    const skip = (this.currentPage - 1) * this.pageSize;
    
    this.recepcionService.getTickets(skip, this.pageSize, this.filtroEstado).subscribe({
      next: (data) => {
        this.tickets = data;
        this.totalItems = 240; // <-- Idealmente esto viene de la API
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error cargando tickets', err);
        this.isLoading = false;
      }
    });
  }

  // --- Métodos para la paginación ---
  irAPagina(pagina: number): void {
    this.currentPage = pagina;
    this.loadTickets();
  }
  
  paginaSiguiente(): void {
    this.currentPage++;
    this.loadTickets();
  }

  paginaAnterior(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadTickets();
    }
  }

  // --- Métodos para filtros ---
  aplicarFiltro(): void {
    this.currentPage = 1;
    this.loadTickets();
  }

  // --- 👇 4. AGREGAR LOS MÉTODOS PARA CREAR ÓRDENES ---
  
  /**
   * Abre el diálogo para crear una orden de un cliente nuevo.
   */
  onNuevaOrdenClienteNuevo() {
    const dialogRef = this.dialogService.open(NuevaOrdenClienteNuevoComponent);
    dialogRef.afterClosed().then(result => {
      if (result) {
        this.loadTickets(); // Recargar lista si se guardó
      }
    });
  }

  /**
   * Abre el diálogo para crear una orden de un cliente existente (legacy).
   */
  onNuevaOrdenClienteLegacy() {
    const dialogRef = this.dialogService.open(NuevaOrdenClienteLegacyComponent);
    dialogRef.afterClosed().then(result => {
      if (result) {
        this.loadTickets(); // Recargar lista si se guardó
      }
    });
  }


  // Helper para las clases de los badges de estado
  getStatusClass(estado: string): string {
    const estadoSeguro = (estado || '').toLowerCase();
    
    switch (estadoSeguro) {
      case 'activo':
        return 'bg-green-100 text-green-800';
      case 'finalizado':
        return 'bg-gray-100 text-gray-800';
      case 'cancelar':
      case 'cancelado':
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-blue-100 text-blue-800';
    }
  }
}

