// divider.component.ts
import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-ui-divider',
    standalone: true,
    imports: [CommonModule],
    template: `
        @if (horizontal == true) {
            <div class=" bg-(--color-linea) h-[1px] w-full"></div>
        }@else {
            <div class=" bg-(--color-linea) h-full w-[1px]"></div>
        }

    `,
})
export class UiDividerComponent {
    @Input() horizontal: boolean = true
}
