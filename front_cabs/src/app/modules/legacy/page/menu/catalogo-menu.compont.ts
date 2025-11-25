import { Component, ChangeDetectionStrategy } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-catalogo-menu',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="p-6 md:p-10">
      <h1 class="text-3xl font-bold text-gray-900 mb-6">
        Catálogos Base
      </h1>
      
      <nav class="flex items-center gap-6 border-b border-gray-300 overflow-x-auto">
        
        <a routerLink="./monedas"
           routerLinkActive="!text-blue-600 !border-blue-600"
           class="py-3 px-1 text-gray-600 font-medium whitespace-nowrap border-b-2 border-transparent transition-colors cursor-pointer hover:text-gray-900 hover:border-gray-300">
           Monedas
        </a>

        <a routerLink="./agentes"
           routerLinkActive="!text-blue-600 !border-blue-600"
           class="py-3 px-1 text-gray-600 font-medium whitespace-nowrap border-b-2 border-transparent transition-colors cursor-pointer hover:text-gray-900 hover:border-gray-300">
           Agentes
        </a>

        <a routerLink="./almacenes"
           routerLinkActive="!text-blue-600 !border-blue-600"
           class="py-3 px-1 text-gray-600 font-medium whitespace-nowrap border-b-2 border-transparent transition-colors cursor-pointer hover:text-gray-900 hover:border-gray-300">
           Almacenes
        </a>

        <a routerLink="./productos"
           routerLinkActive="!text-blue-600 !border-blue-600"
           class="py-3 px-1 text-gray-600 font-medium whitespace-nowrap border-b-2 border-transparent transition-colors cursor-pointer hover:text-gray-900 hover:border-gray-300">
           Productos
        </a>

        <a routerLink="./clientes"
           routerLinkActive="!text-blue-600 !border-blue-600"
           class="py-3 px-1 text-gray-600 font-medium whitespace-nowrap border-b-2 border-transparent transition-colors cursor-pointer hover:text-gray-900 hover:border-gray-300">
           Clientes
        </a>

      </nav>
      
      <div class="mt-6 fade-in">
        <router-outlet />
      </div>
    </div>
  `,
  styles: [`
    /* Animación suave de entrada */
    .fade-in {
      animation: fadeIn 0.3s ease-in-out;
    }
    @keyframes fadeIn {
      from { opacity: 0; transform: translateY(5px); }
      to { opacity: 1; transform: translateY(0); }
    }
  `]
})
export class CatalogoMenuComponent { }