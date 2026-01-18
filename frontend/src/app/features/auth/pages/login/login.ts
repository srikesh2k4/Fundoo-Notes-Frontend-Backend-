import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login {
  private authService = inject(AuthService);
  private router = inject(Router);

  email = '';
  password = '';
  showPassword = false;

  isLoading = signal(false);
  errorMessage = signal('');

  get isFormValid(): boolean {
    return !!(this.email.trim() && this.password);
  }

  toggleShowPassword(): void {
    this.showPassword = !this.showPassword;
  }

  onLoginSubmit(event: Event): void {
    event.preventDefault();

    if (!this.isFormValid) {
      this.errorMessage.set('Please fill in all fields');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set('');

    const loginData = {
      email: this.email.trim(),
      password: this.password
    };

    this.authService.login(loginData).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        if (response.success) {
          this.router.navigate(['/']);
        }
      },
      error: (error) => {
        this.isLoading.set(false);
        const errorMsg = error.error?.message || 'Login failed. Please check your credentials.';

        // Check if email is not verified
        if (errorMsg.toLowerCase().includes('verify') || errorMsg.toLowerCase().includes('otp')) {
          this.errorMessage.set('Please verify your email first. Check your inbox for the OTP.');
        } else {
          this.errorMessage.set(errorMsg);
        }
      }
    });
  }
}
