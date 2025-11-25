import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UitipografiaComponent } from '../../atoms/tipografia/tipografia.component';
import { UiBotonComponent } from '../../atoms/boton/boton.component';
import { UiDividerComponent } from '../../atoms/linea/linea.component';

@Component({
    selector: 'app-ui-header-modal',
    standalone: true,
    imports: [CommonModule,UitipografiaComponent,UiBotonComponent,UiDividerComponent,],
    templateUrl: './header-modal.component.html'
})
export class UiHeaderModalComponent {
    @Input() titulo: string = ''
    @Input() descripcion: string = ''
    @Input() buttonLabel: string = ''

    @Output() buttonClick = new EventEmitter<void>();

    onButtonClick() {
        this.buttonClick.emit();
    }
}