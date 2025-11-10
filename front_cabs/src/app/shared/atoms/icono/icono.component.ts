import { Component, Input } from '@angular/core';

@Component({
    selector: 'app-ui-icono',
    templateUrl: './icono.component.html',
})
export class UiIconComponent {
    @Input() name: string = 'check'; 
    @Input() size: string = 'h-6 w-6'; 
    @Input() color: string = 'text-gray-700'; 
}
