import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.componente.css']
})
export class SettingsComponent {
  private router = inject(Router);

  // Estado de las opciones
  darkMode: boolean = false;
  notifications: boolean = true;
  idioma: string = 'es';
  zonaHoraria: string = 'America/Buenos_Aires';

  // Simulación de guardar configuración
  guardarCambios() {
    console.log('Configuración guardada:', {
      darkMode: this.darkMode,
      notifications: this.notifications,
      idioma: this.idioma,
      zonaHoraria: this.zonaHoraria
    });
    alert('Cambios guardados correctamente.');
  }

  // Restaurar valores por defecto
  restaurarValores() {
    this.darkMode = false;
    this.notifications = true;
    this.idioma = 'es';
    this.zonaHoraria = 'America/Buenos_Aires';
    alert(' Valores restaurados a los predeterminados.');
  }
}
