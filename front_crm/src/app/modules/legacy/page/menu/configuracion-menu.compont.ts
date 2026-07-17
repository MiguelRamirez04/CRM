import { Component, ChangeDetectionStrategy } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { UiHeaderComponent } from '../../../../shared/molecules/header/header.component';

@Component({
  selector: 'app-configuracion-menu',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive,UiHeaderComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="flex flex-col gap-8 w-full">
      <app-ui-header
        titulo="Configuración de Documentos"
        [visualizarButton]="false"
      ></app-ui-header>

      <nav class="flex items-center gap-6 border-b border-gray-300 overflow-x-auto">
        
        <a routerLink="./documentos-modelo"
           routerLinkActive="!text-blue-600 !border-blue-600"
           class="py-3 px-1 text-gray-600 font-medium whitespace-nowrap border-b-2 border-transparent transition-colors cursor-pointer hover:text-gray-900 hover:border-gray-300">
           Modelos
        </a>

        <a routerLink="./conceptos"
           routerLinkActive="!text-blue-600 !border-blue-600"
           class="py-3 px-1 text-gray-600 font-medium whitespace-nowrap border-b-2 border-transparent transition-colors cursor-pointer hover:text-gray-900 hover:border-gray-300">
           Conceptos
        </a>

        <a routerLink="./numeros-serie"
           routerLinkActive="!text-blue-600 !border-blue-600"
           class="py-3 px-1 text-gray-600 font-medium whitespace-nowrap border-b-2 border-transparent transition-colors cursor-pointer hover:text-gray-900 hover:border-gray-300">
           Números de Serie
        </a>

      </nav>

      <div class=" fade-in">
        <router-outlet />
      </div>
    </div>
  `,
  styles: [`
    /* Opcional: Una pequeña animación para cuando cambia la tab */
    .fade-in {
      animation: fadeIn 0.3s ease-in-out;
    }
    @keyframes fadeIn {
      from { opacity: 0; transform: translateY(5px); }
      to { opacity: 1; transform: translateY(0); }
    }
  `]
})
export class ConfiguracionMenuComponent { }