import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrdenFormComponent } from '../../orden-form/orden-form.component';
import { DialogRef } from '../../../services/dialog.service';
import { RecepcionService } from '../../../services/recepcion.service';
import { OrdenTrabajoRequest } from '../../../../../core/models/orden-trabajo.interface';

@Component({
  selector: 'app-nueva-orden-cliente-legacy',
  imports: [CommonModule, OrdenFormComponent],
  templateUrl: './nueva-orden-cliente-legacy.component.html',
  styleUrl: './nueva-orden-cliente-legacy.component.css'
})
export class NuevaOrdenClienteLegacyComponent {
  dialogRef!: DialogRef<boolean>;
  private recepcionService = inject(RecepcionService);
  loading = false;

  onGuardarOrden(ordenRequest: OrdenTrabajoRequest): void {
    this.loading = true;
    this.recepcionService.crearOrden(ordenRequest).subscribe({
      next: (ordenCreada) => {
        console.log('Orden creada exitosamente:', ordenCreada);
        this.loading = false;
        this.dialogRef.close(true);
      },
      error: (err: any) => {
        console.error('Error al crear la orden:', err);
        this.loading = false;
        alert('Error al guardar la orden. Por favor intente nuevamente.');
      }
    });
  }

  onCancelar(): void {
    this.dialogRef.close(false);
  }
}
