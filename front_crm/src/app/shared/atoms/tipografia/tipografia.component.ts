import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

type tipografiaVariantes =
    | 'h1' | 'h2' | 'h3' | 'h4' | 'h5' | 'h6'
    | 'p'  | 'p-lg' | 'p-md' | 'p-sm'
    | 'caption' | 'caption-lg' | 'caption-md' | 'caption-sm';

@Component({
    selector: 'app-ui-tipografia',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './tipografia.component.html'
})
export class UitipografiaComponent {
    // Inputs de la etiqueta app-ui-tipografia
    @Input() variante: tipografiaVariantes = 'p';
    @Input() texto: string = '';
    @Input() color?: string; // opcional: si el dev quiere sobrescribir

    get classes(): string {
        // Jerarquía de colores por defecto
        const base = {
            // h
            h1: 'text-4xl font-bold text-gray-800',
            h2: 'text-3xl font-semibold text-gray-700',
            h3: 'text-2xl font-semibold text-gray-700',
            h4: 'text-xl font-medium text-gray-600',
            h5: 'text-lg font-medium text-gray-600',
            h6: 'text-base font-medium text-gray-500',

            // p
            p: 'text-base text-gray-700',
            'p-lg': 'text-lg text-gray-800',
            'p-md': 'text-base text-gray-700',
            'p-sm': 'text-sm text-gray-600',

            // caption 
            caption: 'text-sm text-gray-500 font-medium',
            'caption-lg': 'text-sm font-medium text-gray-500',
            'caption-md': 'text-xs font-medium text-gray-500',
            'caption-sm': 'text-[11px] font-medium text-gray-400',
        }[this.variante];

        // Si el desarrollador pasa un color, se aplica encima
        return this.color ? `${base ?? ''} ${this.color}` : base ?? '';
    }
}
