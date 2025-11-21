import { Component } from '@angular/core';
import { Sidebar } from '../../layout/sidebar/sidebar'; // ajusta la ruta si es necesario
import { RouterOutlet } from '@angular/router';
@Component({
  selector: 'app-dashboard-layout',
  standalone: true,
  imports: [Sidebar, RouterOutlet, ],
  templateUrl: './dashboard-layout.component.html',
  styleUrls: ['./dashboard-layout.component.css']
})
export class DashboardLayoutComponent {}
