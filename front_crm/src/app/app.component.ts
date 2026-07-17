import { Component, OnDestroy, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CotizacionVencimientoService } from './core/services/cotizacion-vencimiento.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnDestroy {
  title = 'frontend';
  private vencimientoService = inject(CotizacionVencimientoService);

  ngOnDestroy(): void {
    // Limpiar el servicio de vencimientos al destruir el componente
    this.vencimientoService.destruir();
  }
}
