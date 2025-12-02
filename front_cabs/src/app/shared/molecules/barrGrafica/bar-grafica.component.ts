import { Component, Input, OnChanges, Output, EventEmitter} from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgChartsModule } from 'ng2-charts';

import {
  Chart,
  ChartConfiguration,
  ChartType,
  BarElement,
  CategoryScale,
  LinearScale,
  Legend,
  Tooltip,
} from 'chart.js';

// Registrar manualmente lo que usa la gráfica de barras
Chart.register(BarElement, CategoryScale, LinearScale, Legend, Tooltip);

interface DatoMensual {
  mes: string;
  valor: number;
}

@Component({
  selector: 'app-ui-barra-grafica',
  standalone: true,
  imports: [CommonModule, NgChartsModule],
  template: `
    <div class="w-full h-full">
      <canvas
        baseChart
        [data]="barChartData"
        [options]="barChartOptions"
        [type]="barChartType"
        (chartClick)="handleClick($event)">
      </canvas>
    </div>
  `,
})
export class UiBarraGraficaComponent implements OnChanges {
  @Input() datos: DatoMensual[] = [];

  barChartType: ChartType = 'bar';

  barChartData: ChartConfiguration['data'] = {
    labels: [],
    datasets: [],
  };

  barChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,

    plugins: {
      legend: { display: false },
      tooltip: {
        enabled: true,
        backgroundColor: '#fff',
        titleColor: '#2962FF',
        bodyColor: '#333',
        borderColor: '#2962FF',
        borderWidth: 1,
      },
    },

    scales: {
      x: {
        ticks: { color: '#999' },
        grid: { display: false },
      },
      y: {
        beginAtZero: true,
        ticks: { color: '#999' },
        grid: { color: '#eee' },
      },
    },
  };

  ngOnChanges() {
    if (!this.datos) return;

    this.barChartData = {
      labels: this.datos.map(d => d.mes),
      datasets: [
        {
          label: 'Datos mensuales',
          data: this.datos.map(d => d.valor),
          backgroundColor: '#2962FF',
          borderRadius: 6,
          barPercentage: 0.8,
          categoryPercentage: 0.8,
        },
      ],
    };
  }

  handleClick(event: any) {
    const index = event?.active?.[0]?.index;
    if (index == null) return;

    this.barraClickeada.emit(index);
  }

  @Output() barraClickeada = new EventEmitter<number>();

}