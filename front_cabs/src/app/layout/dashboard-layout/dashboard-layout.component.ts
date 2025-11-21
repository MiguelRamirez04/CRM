import { Component } from '@angular/core';
import { Sidebar } from '../../layout/sidebar/sidebar'; // ajusta la ruta si es necesario
import { RouterOutlet } from '@angular/router';
<<<<<<< HEAD

@Component({
  selector: 'app-dashboard-layout',
  standalone: true,
  imports: [Sidebar, RouterOutlet],
=======
@Component({
  selector: 'app-dashboard-layout',
  standalone: true,
  imports: [Sidebar, RouterOutlet, ],
>>>>>>> 29afbe45571ab99f1c722a38a504c27ea9e3be5c
  templateUrl: './dashboard-layout.component.html',
  styleUrls: ['./dashboard-layout.component.css']
})
export class DashboardLayoutComponent {}
