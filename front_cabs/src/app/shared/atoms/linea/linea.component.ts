// divider.component.ts
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-ui-divider',
    standalone: true,
    imports: [CommonModule],
    template: `<div class=" bg-(---color-linea) h-0.5 w-full"></div>`,
})
export class UiDividerComponent {
    
}
