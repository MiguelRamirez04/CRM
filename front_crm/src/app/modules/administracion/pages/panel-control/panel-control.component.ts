import { Component, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { Chart } from 'chart.js';
import { CommonModule } from '@angular/common';
import { UiHeaderComponent } from '../../../../shared/molecules/header/header.component';
import { UiCardComponent} from '../../../../shared/molecules/card/card.component';
import { UiBarraGraficaComponent } from '../../../../shared/molecules/barrGrafica/bar-grafica.component';
import { UiDividerComponent } from '../../../../shared/atoms/linea/linea.component';
import type { VarianteEtiqueta } from '../../../../shared/molecules/etiqueta/etiqueta.component';
import { UitipografiaComponent } from '../../../../shared/atoms/tipografia/tipografia.component';



@Component({
  selector: 'app-panel-control',
  imports: [CommonModule,UiHeaderComponent,UiCardComponent,UiBarraGraficaComponent,UiDividerComponent,UitipografiaComponent],  
  templateUrl: './panel-control.component.html',
})
export class PanelControlComponent {
  @ViewChild('barCanvas') barCanvas!: ElementRef<HTMLCanvasElement>;
  barChart!: Chart;
  ventasCotizadas = [
    { mes: 'Enero', valor: 120 },
    { mes: 'Febrero', valor: 95 },
    { mes: 'Marzo', valor: 130 },
    { mes: 'Abril', valor: 110 },
    { mes: 'Mayo', valor: 140 },
    { mes: 'Junio', valor: 125 },
    { mes: 'Julio', valor: 150 },
    { mes: 'Agosto', valor: 135 },
    { mes: 'Septiembre', valor: 145 },
    { mes: 'Octubre', valor: 160 },
    { mes: 'Noviembre', valor: 155 },
    { mes: 'Diciembre', valor: 180 },
  ];
  documentosSubidos = [
    { mes: 'Enero', valor: 150 },
    { mes: 'Febrero', valor: 95 },
    { mes: 'Marzo', valor: 130 },
    { mes: 'Abril', valor: 110 },
    { mes: 'Mayo', valor: 140 },
    { mes: 'Junio', valor: 125 },
    { mes: 'Julio', valor: 150 },
    { mes: 'Agosto', valor: 135 },
    { mes: 'Septiembre', valor: 145 },
    { mes: 'Octubre', valor: 160 },
    { mes: 'Noviembre', valor: 155 },
    { mes: 'Diciembre', valor: 200 },
  ];
  ventasCanceladas = [
    { mes: 'Enero', valor: 120 },
    { mes: 'Febrero', valor: 95 },
    { mes: 'Marzo', valor: 130 },
    { mes: 'Abril', valor: 110 },
    { mes: 'Mayo', valor: 140 },
    { mes: 'Junio', valor: 125 },
    { mes: 'Julio', valor: 150 },
    { mes: 'Agosto', valor: 135 },
    { mes: 'Septiembre', valor: 145 },
    { mes: 'Octubre', valor: 160 },
    { mes: 'Noviembre', valor: 155 },
    { mes: 'Diciembre', valor: 500 },
  ];


    tarjetas = [
    { id: 1, nameIcono: 'banknotes', titulo: 'Ventas cotizadas', viewSimbolo: true, porcentaje: '', estadoEtiqueta: 'neutro' as VarianteEtiqueta, valor:0, datos: this.ventasCotizadas },
    { id: 2, nameIcono: 'folder', titulo: 'Documentos subidos', viewSimbolo: false, porcentaje: '', estadoEtiqueta: 'neutro' as VarianteEtiqueta, valor:0, datos: this.documentosSubidos },
    { id: 3, nameIcono: 'no-symbol', titulo: 'Ventas canceladas', viewSimbolo: false, porcentaje: '', estadoEtiqueta: 'neutro' as VarianteEtiqueta,valor:0, datos: this.ventasCanceladas },
  ];

  tarjetaSeleccionadaId = 1;

  seleccionarTarjeta(id: number) {
    this.tarjetaSeleccionadaId = id;
    const datos = this.tarjetas.find(t => t.id === id)?.datos || [];
    this.actualizarGrafica(datos);
  }

  get datosSeleccionados() {
    return this.tarjetas.find(t => t.id === this.tarjetaSeleccionadaId)?.datos || [];
  }
  actualizarGrafica(datos: { mes: string; valor: number }[]) {
    const nuevosValores = datos.map(d => d.valor);
    this.barChart.data.datasets[0].data = nuevosValores;
    this.barChart.update();
  }

  actualizarValorTarjeta(indice: number) {
    const tarjeta = this.tarjetas.find(t => t.id === this.tarjetaSeleccionadaId);
    if (!tarjeta) return;

    const datos = tarjeta.datos;
    const actual = datos[indice]?.valor;
    const anterior = indice > 0 ? datos[indice - 1]?.valor : datos[datos.length - 1]?.valor;

    if (actual == null || anterior == null) return;

    tarjeta.valor = actual;

    const cambio = ((actual - anterior) / anterior) * 100;
    tarjeta.porcentaje = `${cambio.toFixed(1)}%`;

    tarjeta.estadoEtiqueta =
      cambio > 0 ? 'positivo' :
      cambio < 0 ? 'negativo' :
      'neutro';
  }
  ngOnInit() {
    this.tarjetas.forEach(t => {
      const datos = t.datos;
      const actual = datos[datos.length - 1]?.valor;
      const anterior = datos[datos.length - 2]?.valor ?? actual;

      const cambio = ((actual - anterior) / anterior) * 100;

      t.valor = actual;
      t.porcentaje = `${cambio.toFixed(1)}%`;
      t.estadoEtiqueta = cambio > 0 ? 'positivo' : 'negativo';
    });
  }

  get tituloSeleccionado(): string {
    return this.tarjetas.find(t => t.id === this.tarjetaSeleccionadaId)?.titulo ?? '';
  }
}
