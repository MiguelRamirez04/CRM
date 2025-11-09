import { Component, Input } from '@angular/core';

@Component({
    selector: 'app-ui-icono',
    templateUrl: './icono.component.html',
})
export class UiIconComponent {
    @Input() name: string = 'check'; // nombre del ícono
    @Input() size: string = 'h-6 w-6'; // clases Tailwind para tamaño
    @Input() color: string = 'text-gray-700'; // clases Tailwind para color

}
