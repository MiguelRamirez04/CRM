import { Component, ChangeDetectionStrategy } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router'; // Agregamos RouterOutlet y RouterLinkActive

@Component({
  selector: 'app-operaciones-menu',
  standalone: true,
  // Importante: Agregar RouterOutlet y RouterLinkActive a los imports
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="p-6 md:p-10">
      
      <h1 class="text-3xl font-bold text-gray-900 mb-6">
        Operaciones (CRUD)
      </h1>

      <nav class="flex items-center gap-6 border-b border-gray-300 overflow-x-auto">
        
        <a routerLink="./documentos"
           routerLinkActive="!text-blue-600 !border-blue-600"
           class="py-3 px-1 text-gray-600 font-medium whitespace-nowrap border-b-2 border-transparent transition-colors cursor-pointer hover:text-gray-900 hover:border-gray-300">
           Documentos
        </a>

        <a routerLink="./movimientos"
           routerLinkActive="!text-blue-600 !border-blue-600"
           class="py-3 px-1 text-gray-600 font-medium whitespace-nowrap border-b-2 border-transparent transition-colors cursor-pointer hover:text-gray-900 hover:border-gray-300">
           Movimientos
        </a>

        <a routerLink="./movimientos-serie"
           routerLinkActive="!text-blue-600 !border-blue-600"
           class="py-3 px-1 text-gray-600 font-medium whitespace-nowrap border-b-2 border-transparent transition-colors cursor-pointer hover:text-gray-900 hover:border-gray-300">
           Movimientos (Serie)
        </a>

      </nav>

      <div class="mt-6 fade-in">
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