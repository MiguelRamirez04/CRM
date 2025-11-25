import { Component, ChangeDetectionStrategy } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-configuracion-menu',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="p-6 md:p-10">
      <h1 class="text-3xl font-bold text-gray-900 mb-6">
        Configuración de Documentos
      </h1>

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

      <div class="mt-6 fade-in">
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