import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.scss',
})
export class Register {
  private authService = inject(AuthService);
  private router = inject(Router);

  // Form state
  currentStep = signal<'register' | 'otp'>('register');

  // Registration form
  firstName = '';
  lastName = '';
  email = '';
  password = '';
  confirmPassword = '';
  showPassword = false;

  // OTP form
  otp = ['', '', '', '', '', ''];
  trackByIndex(index: number): number {
  return index;
}


  // UI state
  isLoading = signal(false);
  errorMessage = signal('');
  successMessage = signal('');

  // Validation
  get isFormValid(): boolean {
    return !!(
      this.firstName.trim() &&
      this.lastName.trim() &&
      this.email.trim() &&
      this.password &&
      this.confirmPassword &&
      this.password === this.confirmPassword &&
      this.password.length >= 8
    );
  }

  get passwordsMatch(): boolean {
    return this.password === this.confirmPassword;
  }

  get isPasswordStrong(): boolean {
    const hasLetter = /[a-zA-Z]/.test(this.password);
    const hasNumber = /[0-9]/.test(this.password);
    const hasSymbol = /[!@#$%^&*(),.?":{}|<>]/.test(this.password);
    return this.password.length >= 8 && hasLetter && hasNumber && hasSymbol;
  }

  toggleShowPassword(): void {
    this.showPassword = !this.showPassword;
  }

  onRegisterSubmit(event: Event): void {
    event.preventDefault();

    if (!this.isFormValid) {
      if (!this.passwordsMatch) {
        this.errorMessage.set('Passwords do not match');
      } else if (this.password.length < 8) {
        this.errorMessage.set('Password must be at least 8 characters');
      }
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set('');

    const registerData = {
      name: `${this.firstName.trim()} ${this.lastName.trim()}`,
      email: this.email.trim(),
      password: this.password
    };

    this.authService.register(registerData).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        if (response.success) {
          this.successMessage.set(response.message);
          this.currentStep.set('otp');
        }
      },
      error: (error) => {
        this.isLoading.set(false);
        this.errorMessage.set(error.error?.message || 'Registration failed. Please try again.');
      }
    });
  }

  onOtpInput(event: Event, index: number): void {
  const input = event.target as HTMLInputElement;
  const value = input.value;

  // Allow only digits
  if (!/^\d?$/.test(value)) {
    input.value = '';
    return;
  }

  this.otp[index] = value;

  // Move forward only if a digit is entered
  if (value && index < this.otp.length - 1) {
    const next = document.getElementById(`otp-${index + 1}`) as HTMLInputElement;
    next?.focus();
  }
}


onOtpKeydown(event: KeyboardEvent, index: number): void {
  const input = event.target as HTMLInputElement;

  if (event.key === 'Backspace') {
    if (input.value) {
      // Clear current digit
      this.otp[index] = '';
      return;
    }

    if (index > 0) {
      const prev = document.getElementById(`otp-${index - 1}`) as HTMLInputElement;
      prev?.focus();
      this.otp[index - 1] = '';
    }
  }
}



onOtpPaste(event: ClipboardEvent): void {
  event.preventDefault();
  const digits = event.clipboardData
    ?.getData('text')
    .replace(/\D/g, '')
    .slice(0, 6);

  if (!digits) return;

  digits.split('').forEach((digit, i) => {
    this.otp[i] = digit;
    const input = document.getElementById(`otp-${i}`) as HTMLInputElement;
    if (input) input.value = digit;
  });

  const lastInput = document.getElementById(`otp-${digits.length - 1}`) as HTMLInputElement;
  lastInput?.focus();
}


  get isOtpComplete(): boolean {
    return this.otp.every(digit => digit !== '');
  }

  verifyOtp(): void {
    if (!this.isOtpComplete) return;

    this.isLoading.set(true);
    this.errorMessage.set('');

    const otpData = {
      email: this.email,
      otp: this.otp.join('')
    };

    this.authService.verifyOtp(otpData).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        if (response.success) {
          this.successMessage.set('Email verified successfully! Redirecting to login...');
          setTimeout(() => {
            this.router.navigate(['/login']);
          }, 2000);
        }
      },
      error: (error) => {
        this.isLoading.set(false);
        this.errorMessage.set(error.error?.message || 'Invalid OTP. Please try again.');
      }
    });
  }

  resendOtp(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.authService.resendOtp(this.email).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        this.successMessage.set('OTP resent successfully!');
        this.otp = ['', '', '', '', '', ''];
      },
      error: (error) => {
        this.isLoading.set(false);
        this.errorMessage.set(error.error?.message || 'Failed to resend OTP.');
      }
    });
  }

  goBackToRegister(): void {
    this.currentStep.set('register');
    this.otp = ['', '', '', '', '', ''];
    this.errorMessage.set('');
    this.successMessage.set('');
  }
}
