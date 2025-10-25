import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.html'
})
export class Sidebar {
secondaryNav: any;
mostrarBandejaPerfil: any;
isActive(arg0: any) {
throw new Error('Method not implemented.');
}
isCollapsed: any;
mainNav: any;
toggleSidebar() {
throw new Error('Method not implemented.');
}
}
