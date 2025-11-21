import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UitipografiaComponent } from '../../atoms/tipografia/tipografia.component';
import { UiBotonComponent } from '../../atoms/boton/boton.component';


@Component({
    selector: 'app-ui-header',
    standalone: true,
    imports: [CommonModule,UitipografiaComponent,UiBotonComponent],
    templateUrl: './header.component.html'
})
export class UiHeaderComponent {
    @Input() titulo: string = ''
    @Input() descripcion: string = ''
    @Input() buttonLabel: string = ''

    @Input() visualizarDescripcion: boolean = true;
    @Input() visualizarButton: boolean = true;

}