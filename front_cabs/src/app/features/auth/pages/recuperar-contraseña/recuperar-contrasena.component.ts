import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UitipografiaComponent} from '../../../../shared/atoms/tipografia/tipografia.component'
import { UiInputComponent } from '../../../../shared/atoms/input/input.component';
import { UiDividerComponent } from '../../../../shared/atoms/linea/linea.component';

@Component({
    selector: 'app-recuperar-contrasena',
    standalone: true,
    imports: [CommonModule,UiDividerComponent],
    template: `
<div class=" gap-6 w-full  bg-white h-auto flex flex-col justify-between pb-8 text-zinc-800 ">

    <!-- Header -->
    <header class=" flex-col justify-between items-center bg-blue-50 ">
        <div class="flex justify-between items-center mb-4 mx-6 mt-4">
            <div>
                <h3 class="text-xl font-semibold text-black">¿Olvidaste tu contraseña?</h3>
                <p class="text-sm text-zinc-600">Recupera el acceso a tu cuenta de forma segura</p>
            </div>
            <img src="/logoCompleto.png" alt="Logo de la empresa" class="w-[150px]" />
        </div>            
        <app-ui-divider></app-ui-divider>
    </header>

    <!-- Contenido -->
    <main class="flex flex-col gap-6  mx-6">
        <p class="text-sm leading-relaxed">
        Hola <strong>$nombreUsuario</strong>,<br />
        Recibimos una solicitud para restablecer tu contraseña. Para continuar, ingresa el siguiente código de verificación en la página de recuperación:
        </p>

        <!-- Código de verificación -->
        <div class="flex justify-center gap-4">
            <span class="w-10 h-10 flex items-center justify-center text-lg font-bold bg-white border border-blue-500 rounded-md shadow-sm">4</span>
            <span class="w-10 h-10 flex items-center justify-center text-lg font-bold bg-white border border-blue-500 rounded-md shadow-sm">8</span>
            <span class="w-10 h-10 flex items-center justify-center text-lg font-bold bg-white border border-blue-500 rounded-md shadow-sm">2</span>
            <span class="w-10 h-10 flex items-center justify-center text-lg font-bold bg-white border border-blue-500 rounded-md shadow-sm">9</span>
            <span class="w-10 h-10 flex items-center justify-center text-lg font-bold bg-white border border-blue-500 rounded-md shadow-sm">1</span>
            <span class="w-10 h-10 flex items-center justify-center text-lg font-bold bg-white border border-blue-500 rounded-md shadow-sm">3</span>
        </div>

        <p class="text-sm text-zinc-600">
        Este código es válido por los próximos <strong>10 minutos</strong>. Si no solicitaste este cambio, puedes ignorar este mensaje.
        </p>
    </main>

    <app-ui-divider></app-ui-divider>
    <!-- Footer -->
    <footer class="text-xs text-zinc-500 text-center  ">
        © 2025 CABS Computación. Este correo fue enviado automáticamente.<br />
        Si tienes dudas, contáctanos en <a href="mailto:soporte@cabsgo.com" class="text-blue-600 underline">soporte@cabsgo.com</a>
    </footer>
</div>
    `
})
export class RecuperarContasenaComponent {
}