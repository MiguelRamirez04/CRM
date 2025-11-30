import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef } from '@angular/material/dialog';
import { UiHeaderModalComponent } from '../../../molecules/headerModal/header-modal.component'; 
import { UiInputComponent } from '../../../molecules/input/input.component';


@Component({
  selector: 'app-crear-usuario',
  standalone: true,
  imports: [CommonModule, UiHeaderModalComponent, UiInputComponent],
  templateUrl: './crear-modal.component.html',
})
export class ModalCrearUsuario {
  constructor(public dialogRef: MatDialogRef<ModalCrearUsuario>) {}

  cerrar(): void {
    this.dialogRef.close();
  }
  rolesSeleccionado: string = '';
  // Opciones del input select 
  roles = [
    {value: '', label: "Selecione una opción"},
    {value: 'ADMINISTRADOR', label: 'Administrador'},
    {value: 'SOPORTE', label: 'Soporte'},
    {value: 'RECEPCION', label: 'Recepcion'}
  ];


}
