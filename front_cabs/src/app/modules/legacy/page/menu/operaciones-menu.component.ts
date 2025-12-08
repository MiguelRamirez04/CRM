import { Component, ChangeDetectionStrategy } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router'; // Agregamos RouterOutlet y RouterLinkActive
import { UiHeaderComponent } from '../../../../shared/molecules/header/header.component';
@Component({
  selector: 'app-operaciones-menu',
  standalone: true,
  // Importante: Agregar RouterOutlet y RouterLinkActive a los imports
  imports: [RouterOutlet, RouterLink, RouterLinkActive, UiHeaderComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="flex flex-col gap-8 w-full">
      <app-ui-header
        titulo="Operaciones (CRUD)"
        [visualizarButton]="false"
      ></app-ui-header>

      <nav class="flex items-center gap-6 border-b border-gray-300 overflow-x-auto">
        
        <a routerLink="./documentos"
           routerLinkActive="!text-blue-600 !border-blue-600"
           class="py-3 px-1 text-gray-600 font-medium whitespace-nowrap border-b-2 border-transparent transition-colors cursor-pointer hover:text-gray-900 hover:border-gray-300">
           Documentos
        </a>

        <a routerLink="./metricas-documentos"
           routerLinkActive="!text-blue-600 !border-blue-600"
           class="py-3 px-1 text-gray-600 font-medium whitespace-nowrap border-b-2 border-transparent transition-colors cursor-pointer hover:text-gray-900 hover:border-gray-300">
           Metricas
        </a>

        <a routerLink="./movimientos-serie"
           routerLinkActive="!text-blue-600 !border-blue-600"
           class="py-3 px-1 text-gray-600 font-medium whitespace-nowrap border-b-2 border-transparent transition-colors cursor-pointer hover:text-gray-900 hover:border-gray-300">
           Movimientos (Serie)
        </a>

      </nav>

      <div class=" fade-in">
        <router-outlet />
      </div>
    </div>
  `,
  styles: [`
    .fade-in {
      animation: fadeIn 0.3s ease-in-out;
    }
    @keyframes fadeIn {
      from { opacity: 0; transform: translateY(5px); }
      to { opacity: 1; transform: translateY(0); }
    }
  `]
})
export class OperacionesMenuComponent { }