import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ThemeService } from '../../../../core/services/theme.service';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.componente.css']
})
export class SettingsComponent {
  private router = inject(Router);
  private themeService = inject(ThemeService);

  darkMode = this.themeService.darkMode;
  notifications: boolean = true;
  idioma: string = 'es';
  zonaHoraria: string = 'America/Buenos_Aires';

  guardarCambios() {
    console.log('Configuración guardada:', {
      darkMode: this.darkMode(),
      notifications: this.notifications,
      idioma: this.idioma,
      zonaHoraria: this.zonaHoraria
    });
    alert('Cambios guardados correctamente.');
  }

  onDarkModeChange() {
    this.themeService.setDarkMode(this.darkMode());
  }

  restaurarValores() {
    this.themeService.setDarkMode(false);
    this.darkMode.set(false);
    this.notifications = true;
    this.idioma = 'es';
    this.zonaHoraria = 'America/Buenos_Aires';
    alert(' Valores restaurados a los predeterminados.');
  }
}
