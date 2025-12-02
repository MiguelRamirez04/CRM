import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UiHeaderComponent } from '../../../../shared/molecules/header/header.component';

@Component({
  selector: 'app-usuarios',
  standalone: true,
  imports: [CommonModule, UiHeaderComponent],
  templateUrl: './usuarios.component.html',
  styleUrls: ['./usuarios.component.css']
})
export class UsuariosComponent {
abrirModalCrear() {
throw new Error('Method not implemented.');
}
  titulo: string = 'Hola';
  descripcion: string = 'Este es el módulo de gestión de usuarios';
}
