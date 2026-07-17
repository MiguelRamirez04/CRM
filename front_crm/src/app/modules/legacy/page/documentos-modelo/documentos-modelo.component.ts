import { Component, ChangeDetectionStrategy } from '@angular/core';
import { RouterLink } from '@angular/router'; // Para el botón "Volver"

@Component({
  selector: 'app-documentos-modelo.component',
  standalone: true,
  imports: [RouterLink], // Importamos RouterLink
  template: `
    <div class="bg-white min-h-screen p-6 md:p-10 font-sans">
      <div class="max-w-7xl mx-auto">
        
        <!-- Botón para regresar al menú -->
        <a routerLink="/legacy" class="back-button">
          &larr; Volver al Menú de Catálogos
        </a>

        <!-- Título -->
        <h1 class="placeholder-title">Catálogo de documentos-modelo.component</h1>
        
        <!-- Contenido de la página -->
        <div class="placeholder-content">
          <p class="api-endpoint">API Endpoint: <strong>GET /api/documentos-modelo.component</strong></p>
          <p>
            Aquí iría tu componente real para mostrar la tabla (datagrid) 
            con la información de los almacenes (Vwdocumentos-modelo.componentCompletas).
          </p>
          
          <!-- (Aquí puedes empezar a construir tu tabla) -->
          <div class="mt-6 p-4 border rounded bg-gray-50">
            Contenido de la tabla de documentos...
          </div>
        </div>

      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DocumentosModeloComponent {
  // Aquí irá la lógica para llamar al servicio que trae los datos de /api/
}