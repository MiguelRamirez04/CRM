import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

type tipografiaVariantes = 'h1' | 'h2' | 'h3' | 'h4' | 'h5' | 'h6' | 'p' | 'caption';

@Component({
    selector: 'app-ui-tipografia',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './tipografia.component.html'
})
export class UitipografiaComponent {
    @Input() variante: tipografiaVariantes = 'p';
    @Input() texto: string = '';
    @Input() color: string = 'text-gray-800';
    get classes(): string {
        const base = {
            h1: 'text-4xl font-bold',
            h2: 'text-3xl font-semibold',
            h3: 'text-2xl font-semibold',
            h4: 'text-xl font-medium',
            h5: 'text-lg font-medium',
            h6: 'text-base font-medium',
            p: 'text-base',
            caption: 'text-sm text-gray-500 font-medium',
        }[this.variante];
        return `${base} ${this.color}`;
    }
}