// =====================================================================================
// DIRECTIVA CLICK OUTSIDE - click-outside.directive.ts
// =====================================================================================
//
// ¿QUÉ HACE ESTA DIRECTIVA?
// Detecta clics fuera del elemento y ejecuta una acción.
// Útil para cerrar dropdowns, modales, etc.
//
// FUNCIONALIDADES:
// - Escucha eventos de clic globales
// - Verifica si el clic fue fuera del elemento
// - Ejecuta la función especificada
//
// =====================================================================================

import { Directive, ElementRef, EventEmitter, Output, HostListener } from '@angular/core';

@Directive({
  selector: '[clickOutside]',
  standalone: true
})
export class ClickOutsideDirective {
  @Output() clickOutside = new EventEmitter<void>();

  constructor(private elementRef: ElementRef) {}

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event): void {
    const targetElement = event.target as HTMLElement;

    // Verificar si el clic fue fuera del elemento
    if (!this.elementRef.nativeElement.contains(targetElement)) {
      this.clickOutside.emit();
    }
  }
}