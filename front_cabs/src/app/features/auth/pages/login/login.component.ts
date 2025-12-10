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
  imports: [
    CommonModule, 
    ReactiveFormsModule, 
    RouterLink
  ],
  templateUrl: './login.component.html'})
export class LoginComponent implements OnInit, OnDestroy {
  // =====================================================================================
  // SERVICIOS E INYECCIONES
  // =====================================================================================
  private fb = inject(FormBuilder);
  private authService = inject(SecureAuthService);
  private router = inject(Router);

  // =====================================================================================
  // PROPIEDADES DE ESTADO
  // =====================================================================================
  loginForm: FormGroup;
  isLoading = false;
  showPassword = false;
  
  // Mensajes de Feedback
  errorMessage: string | null = null; 
  successMessage: string | null = null;
  
  // =====================================================================================
  // CONTROL DE CARRUSEL (Panel Izquierdo)
  // =====================================================================================
  currentSlide = 1;
  private carouselSubscription!: Subscription;

  constructor() {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      rememberMe: [false]
    });
  }

  // =====================================================================================
  // LIFECYCLE HOOKS
  // =====================================================================================
  ngOnInit(): void {
    this.startCarousel();
  }

  ngOnDestroy(): void {
    if (this.carouselSubscription) {
      this.carouselSubscription.unsubscribe();
    }
  }

  // =====================================================================================
  // LÓGICA DE NEGOCIO
  // =====================================================================================

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
   * Alterna la visibilidad del campo de contraseña.
   */
  togglePassword(): void {
    this.showPassword = !this.showPassword;
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
    this.errorMessage = null; 
    this.successMessage = null;

    const loginData = {
        email: this.loginForm.get('email')?.value,
        password: this.loginForm.get('password')?.value
    };

    // 2. Llamada al servicio de autenticación (Backend)
    this.authService.login(loginData).subscribe({
        next: () => {
            this.isLoading = false;
            this.successMessage = 'Inicio de sesión exitoso';
            
            // Redirección al dashboard con un pequeño delay para mostrar el éxito
            setTimeout(() => {
                this.router.navigate(['/dashboard']);
            }, 1000);
        },
        error: (error) => {
          this.isLoading = false;
          // Mensaje amigable para el usuario
          this.errorMessage = 'No se pudo iniciar sesión. Verifica tu usuario y contraseña e inténtalo de nuevo.';
          // Limpiar la contraseña en el formulario después de un error por seguridad
          this.loginForm.patchValue({ password: '' });
          console.error('Login error:', error);
        }
    });
  }
}