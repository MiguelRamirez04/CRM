import { Injectable, ComponentRef, ApplicationRef, createComponent, EnvironmentInjector, Type } from '@angular/core';

export interface DialogConfig<T = any> {
  data?: T;
  width?: string;
  maxWidth?: string;
  closeOnBackdropClick?: boolean;
}

export interface DialogRef<T = any> {
  close: (result?: T) => void;
  afterClosed: () => Promise<T | undefined>;
}

@Injectable({
  providedIn: 'root'
})
export class DialogService {
  private dialogComponentRef: ComponentRef<any> | null = null;
  private overlayElement: HTMLElement | null = null;

  constructor(
    private appRef: ApplicationRef,
    private injector: EnvironmentInjector
  ) {}

  open<T, R = any>(component: Type<any>, config: DialogConfig<T> = {}): DialogRef<R> {
    // Crear overlay
    this.overlayElement = document.createElement('div');
    this.overlayElement.className = 'fixed inset-0 z-[99999] flex items-center justify-center p-4 sm:p-6 md:p-8';
    this.overlayElement.style.backgroundColor = 'rgba(0, 0, 0, 0.75)';
    this.overlayElement.style.backdropFilter = 'blur(8px)';
    
    // Crear componente
    this.dialogComponentRef = createComponent(component, {
      environmentInjector: this.injector
    });

    // Pasar datos al componente si existen
    if (config.data) {
      Object.assign(this.dialogComponentRef.instance, config.data);
    }

    // Adjuntar al DOM
    this.appRef.attachView(this.dialogComponentRef.hostView);
    const domElement = (this.dialogComponentRef.hostView as any).rootNodes[0] as HTMLElement;
    this.overlayElement.appendChild(domElement);
    document.body.appendChild(this.overlayElement);
    
    // Prevenir scroll del body
    document.body.classList.add('modal-open');

    // Crear DialogRef
    let resolvePromise: (value: R | undefined) => void;
    const afterClosedPromise = new Promise<R | undefined>(resolve => {
      resolvePromise = resolve;
    });

    const dialogRef: DialogRef<R> = {
      close: (result?: R) => {
        this.close();
        resolvePromise(result);
      },
      afterClosed: () => afterClosedPromise
    };

    // Pasar dialogRef al componente
    if (this.dialogComponentRef.instance) {
      this.dialogComponentRef.instance.dialogRef = dialogRef;
    }

    // Click en overlay para cerrar (si está habilitado)
    if (config.closeOnBackdropClick !== false) {
      this.overlayElement.addEventListener('click', (event) => {
        if (event.target === this.overlayElement) {
          dialogRef.close();
        }
      });
    }

    return dialogRef;
  }

  private close(): void {
    if (this.dialogComponentRef) {
      this.appRef.detachView(this.dialogComponentRef.hostView);
      this.dialogComponentRef.destroy();
      this.dialogComponentRef = null;
    }

    if (this.overlayElement) {
      document.body.removeChild(this.overlayElement);
      this.overlayElement = null;
    }

    // Restaurar scroll del body
    document.body.classList.remove('modal-open');
  }
}
