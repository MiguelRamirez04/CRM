import { Component, ChangeDetectionStrategy } from '@angular/core';
import { RouterLink } from '@angular/router'; // Para el botón "Volver"

@Component({
  selector: 'app-almacenes',
  standalone: true,
  imports: [RouterLink], // Importamos RouterLink
  template: `
    <div class="bg-white min-h-screen p-6 md:p-10 font-sans">
      <div class="max-w-7xl mx-auto">
        
        <!-- Botón para regresar al menú -->
        <a routerLink="/legacy" class="text-blue-600 hover:text-blue-800 font-medium mb-6 flex items-center gap-2 transition-colors">
          &larr; Volver al Menú de Catálogos
        </a>

        <!-- Título -->
        <h1 class="text-3xl font-bold text-gray-900 mb-6">Catálogo de Almacenes</h1>
        
        <!-- Contenido de la página -->
        <div class="bg-white p-8 rounded-lg shadow-md border border-gray-200 text-gray-700 leading-relaxed">
          <p class="text-sm text-gray-500 bg-gray-100 p-3 rounded-md inline-block border">API Endpoint: <strong>GET /api/Almacenes</strong></p>
          <p>
            Aquí iría tu componente real para mostrar la tabla (datagrid) 
            con la información de los almacenes (VwAlmacenesCompletas).
          </p>
          
          <!-- (Aquí puedes empezar a construir tu tabla) -->
          <div class="mt-6 p-4 border rounded bg-gray-50">
            Contenido de la tabla de almacenes...
          </div>
        </div>

      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AlmacenesComponent {
  // Aquí irá la lógica para llamar al servicio que trae los datos de /api/Almacenes
}