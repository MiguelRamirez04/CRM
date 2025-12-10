import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { interval, Subscription } from 'rxjs'; 
import { startWith } from 'rxjs/operators';


import { SecureAuthService } from '../../../../core/services/secure-auth.service';


import { UiInputComponent } from '../../../../shared/molecules/input/input.component'; 
import { UiBotonComponent } from '../../../../shared/atoms/boton/boton.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule, 
    ReactiveFormsModule, 
    RouterLink,
    UiInputComponent, 
    UiBotonComponent  
  ],
  templateUrl: './login.component.html',
})
export class LoginComponent implements OnInit, OnDestroy {
  // INYECCIONES
  private fb = inject(FormBuilder);
  private authService = inject(SecureAuthService);
  private router = inject(Router);

  // ESTADO
  loginForm: FormGroup;
  isLoading = false;
  
  errorMessage: string | null = null; 
  successMessage: string | null = null;
  
  // CARRUSEL
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
    this.carouselSubscription = interval(4000)
      .pipe(startWith(0))
      .subscribe(() => {
        this.currentSlide = (this.currentSlide % 3) + 1; 
      });
  }

  ngOnDestroy(): void {
    if (this.carouselSubscription) {
      this.carouselSubscription.unsubscribe();
    }
  }

  onSubmit(): void {
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

    this.authService.login(loginData).subscribe({
        next: () => {
            this.isLoading = false;
            this.successMessage = 'Inicio de sesión exitoso';
            setTimeout(() => {
                this.router.navigate(['/dashboard']);
            }, 1000);
        },
        error: (error) => {
          this.isLoading = false;
          this.errorMessage = 'No se pudo iniciar sesión. Verifica tu usuario y contraseña.';
          this.loginForm.patchValue({ password: '' });
        }
    });
  }
}