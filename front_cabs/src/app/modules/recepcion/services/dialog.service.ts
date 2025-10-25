import { Injectable, inject } from '@angular/core';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { Observable } from 'rxjs';
import { NuevaOrdenClienteLegacyComponent } from '../components/dialogs/nueva-orden-cliente-legacy/nueva-orden-cliente-legacy.component';
import { NuevaOrdenClienteNuevoComponent } from '../components/dialogs/nueva-orden-cliente-nuevo/nueva-orden-cliente-nuevo.component';
import { EditarOrdenComponent } from '../components/dialogs/editar-orden/editar-orden.component';
import { OrdenTrabajo } from '../../../core/models/orden-trabajo.interface';

@Injectable({
  providedIn: 'root'
})
export class DialogService {
  private dialog = inject(MatDialog);

  /**
   * Abre el diálogo para crear una nueva orden con un cliente existente (legacy)
   * @returns Observable con la orden creada o undefined si se cancela
   */
  openNuevaOrdenLegacy(): Observable<OrdenTrabajo | undefined> {
    const dialogConfig: MatDialogConfig = {
      width: '90vw',
      maxWidth: '1000px',
      maxHeight: '90vh',
      disableClose: false,
      panelClass: 'custom-dialog-container',
      autoFocus: true,
      restoreFocus: true
    };

    const dialogRef = this.dialog.open(NuevaOrdenClienteLegacyComponent, dialogConfig);
    return dialogRef.afterClosed();
  }

  /**
   * Abre el diálogo para crear una nueva orden con un cliente nuevo
   * @returns Observable con la orden creada o undefined si se cancela
   */
  openNuevaOrdenNuevo(): Observable<OrdenTrabajo | undefined> {
    const dialogConfig: MatDialogConfig = {
      width: '90vw',
      maxWidth: '1000px',
      maxHeight: '90vh',
      disableClose: false,
      panelClass: 'custom-dialog-container',
      autoFocus: true,
      restoreFocus: true
    };

    const dialogRef = this.dialog.open(NuevaOrdenClienteNuevoComponent, dialogConfig);
    return dialogRef.afterClosed();
  }

  /**
   * Abre el diálogo para editar una orden existente
   * @param orden La orden a editar
   * @returns Observable con la orden actualizada o undefined si se cancela
   */
  openEditarOrden(orden: OrdenTrabajo): Observable<OrdenTrabajo | undefined> {
    const dialogConfig: MatDialogConfig = {
      width: '90vw',
      maxWidth: '1000px',
      maxHeight: '90vh',
      disableClose: false,
      panelClass: 'custom-dialog-container',
      autoFocus: true,
      restoreFocus: true,
      data: orden // Pasar la orden directamente como dato
    };

    const dialogRef = this.dialog.open(EditarOrdenComponent, dialogConfig);
    return dialogRef.afterClosed();
  }

  /**
   * Cierra todos los diálogos abiertos
   */
  closeAll(): void {
    this.dialog.closeAll();
  }
}
