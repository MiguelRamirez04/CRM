import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { interval, Subscription } from 'rxjs'; 
import { startWith } from 'rxjs/operators';

import { SecureAuthService } from '../../../../core/services/secure-auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit, OnDestroy {
  // Inyección de servicios
  private fb = inject(FormBuilder);
  private authService = inject(SecureAuthService);
  private router = inject(Router);

  // Propiedades de estado
  loginForm: FormGroup;
  isLoading = false;
  showPassword = false;
  
  // CRÍTICO: Esta variable activa la alerta en el HTML
  errorMessage: string | null = null; 
  
  successMessage: string | null = null;
  
  // Lógica para el carrusel
  currentSlide = 1;
  private carouselSubscription!: Subscription;

  constructor() {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      rememberMe: [false]
    });
  }

  ngOnInit(): void {
    this.startCarousel();
  }

  ngOnDestroy(): void {
    if (this.carouselSubscription) {
      this.carouselSubscription.unsubscribe();
    }
  }

  /**
   * Obtiene la URL de la imagen de mockup según el slide actual.
   */
  getImageUrl(slide: number): string {
    switch (slide) {
      case 1:
        return 'assets/img/dashboard-mockup.jpg';
      case 2:
        return 'assets/img/support-mockup.jpg';
      case 3:
        return 'assets/img/reception-mockup.jpg';
      default:
        return '';
    }
  }

  /**
   * Inicia el temporizador para cambiar de slide automáticamente.
   */
  startCarousel(): void {
    this.carouselSubscription = interval(4000)
      .pipe(startWith(0))
      .subscribe(() => {
        this.currentSlide = (this.currentSlide % 3) + 1; 
      });
  }

  /**
   * Maneja el envío del formulario de inicio de sesión.
   */
  onSubmit(): void {
    // 1. Manejo de errores de validación del formulario (Frontend)
    if (this.loginForm.invalid) {
        this.loginForm.markAllAsTouched();
        this.errorMessage = 'Por favor, completa correctamente los campos requeridos.';
        return;
    }

    this.isLoading = true;
    this.errorMessage = null; // Reiniciar antes de la llamada al backend

    const loginData = {
        email: this.loginForm.get('email')?.value,
        password: this.loginForm.get('password')?.value
    };

    // 2. Llamada al servicio de autenticación (Backend)
    this.authService.login(loginData).subscribe({
        next: () => {
            this.isLoading = false;
            this.successMessage = 'Inicio de sesión exitoso';
            
            // Redirección al dashboard
            setTimeout(() => {
                this.router.navigate(['/dashboard']);
            }, 1000);
        },
        // En login.component.ts (dentro de onSubmit, bloque error)
        error: (error) => {
          this.isLoading = false;
          
          //  ASIGNACIÓN FORZADA DEL MENSAJE LARGO DESEADO 
          this.errorMessage = 'No se pudo iniciar sesión. Verifica tu usuario y contraseña e inténtalo de nuevo.';
          // Limpiar la contraseña en el formulario después de un error
          this.loginForm.patchValue({ password: '' });
        }
    });
  }

  /**
   * Alterna la visibilidad del campo de contraseña.
   */
  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }
}


