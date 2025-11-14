import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UitipografiaComponent } from '../../atoms/tipografia/tipografia.component';
import { UiIconComponent } from "../../atoms/icono/icono.component";
import { UiEtiquetaComponent } from '../etiqueta/etiqueta.component';

type VarianteEtiqueta = 'positivo' | 'negativo' | 'neutro';


@Component({
    selector: 'app-ui-card',
    standalone: true,
    imports: [CommonModule, UitipografiaComponent, UiIconComponent, UiEtiquetaComponent],
    templateUrl: './card.component.html'
})
export class UiCardComponent {
    @Input() nameIcono?: string = ''
    @Input() title?: string = ''
    @Input() valor?: number | number[] = 0
    @Input() viewLabel: boolean = false
    @Input() viewSimbolo: boolean = false
    @Input() porcentaje: string = '';
    @Input() estadoEtiqueta: VarianteEtiqueta= "neutro"
    @Output() seleccionar = new EventEmitter<void>();
    @Input() selected: boolean = false;

}
