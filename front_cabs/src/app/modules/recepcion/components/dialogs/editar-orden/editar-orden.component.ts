import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrdenFormComponent } from '../../orden-form/orden-form.component';
import { DialogRef } from '../../../services/dialog.service';
import { OrdenTrabajo, OrdenTrabajoRequest, OrdenTrabajoUpdateRequest } from '../../../../../core/models/orden-trabajo.interface';
import { RecepcionService } from '../../../services/recepcion.service';

@Component({
  selector: 'app-editar-orden',
  imports: [CommonModule, OrdenFormComponent],
  templateUrl: './editar-orden.component.html',
  styleUrl: './editar-orden.component.css'
})
export class EditarOrdenComponent {
  dialogRef!: DialogRef<boolean>;
  @Input() orden!: OrdenTrabajo;
  private recepcionService = inject(RecepcionService);
  loading = false;

  onGuardarEdicion(ordenRequest: OrdenTrabajoRequest): void {
    if (!this.orden?.id) {
      console.error('No se pudo identificar la orden a editar.');
      return;
    }

    this.loading = true;
    const updateRequest: OrdenTrabajoUpdateRequest = {
      requestDto: ordenRequest.requestDto
    };

    this.recepcionService.actualizarOrden(this.orden.id, updateRequest).subscribe({
      next: () => {
        console.log('Orden actualizada exitosamente');
        this.loading = false;
        this.dialogRef.close(true);
      },
      error: (err: any) => {
        console.error('Error al actualizar la orden:', err);
        this.loading = false;
        alert('Error al actualizar la orden. Por favor intente nuevamente.');
      }
    });
  }

  onCancelar(): void {
    this.dialogRef.close(false);
  }
}
