import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container-fluid">
      <h2>Chat de Soporte</h2>
      <p>Sistema de chat en tiempo real.</p>
      <div class="alert alert-info">
        Chat de soporte próximamente disponible.
      </div>
    </div>
  `
})
export class ChatComponent {}