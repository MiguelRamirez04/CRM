import { Component, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import {
  Chart,
  ChartConfiguration,
  ChartType,
  registerables
} from 'chart.js';

@Component({
  selector: 'app-panel-control',
  templateUrl: './panel-control.component.html',
  styleUrls: ['./panel-control.component.css']
})
export class PanelControlComponent implements AfterViewInit {
  @ViewChild('barCanvas') barCanvas!: ElementRef<HTMLCanvasElement>;
  barChart!: Chart;

  ngAfterViewInit(): void {
    Chart.register(...registerables);

    const config: ChartConfiguration = {
      type: 'bar' as ChartType,
      data: {
        labels: [
          'Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun',
          'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'
        ],
        datasets: [{
          label: 'Datos mensuales',
          data: [120, 90, 150, 80, 130, 100, 170, 140, 110, 160, 95, 125],
          backgroundColor: '#2962FF',
          borderRadius: 4,
          barPercentage: 0.8,
          categoryPercentage: 0.8
        }]
      },
      options: {
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false },
          tooltip: {
            enabled: true,
            backgroundColor: '#fff',
            titleColor: '#2962FF',
            bodyColor: '#333',
            borderColor: '#2962FF',
            borderWidth: 1
          }
        },
        scales: {
          x: {
            ticks: {
              color: '#999'
            },
            grid: {
              display: false
            }
          },
          y: {
            beginAtZero: true,
            ticks: {
              color: '#999'
            },
            grid: {
              color: '#eee'
            }
          }
        }
      }
    };

    this.barChart = new Chart(this.barCanvas.nativeElement, config);
  }
}
