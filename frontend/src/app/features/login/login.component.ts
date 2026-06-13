import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { UserRole } from '../../core/models/models';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  private auth = inject(AuthService);
  private router = inject(Router);

  readonly UserRole = UserRole;

  mode = signal<'login' | 'register'>('login');
  loading = signal(false);
  error = signal<string | null>(null);

  // الحقول
  fullName = '';
  email = '';
  password = '';
  role: UserRole = UserRole.Employee;

  toggleMode(): void {
    this.error.set(null);
    this.mode.set(this.mode() === 'login' ? 'register' : 'login');
  }

  submit(): void {
    if (!this.email || !this.password) {
      this.error.set('الرجاء إدخال البريد وكلمة المرور');
      return;
    }
    this.loading.set(true);
    this.error.set(null);

    const obs = this.mode() === 'login'
      ? this.auth.login(this.email, this.password)
      : this.auth.register(this.fullName, this.email, this.password, this.role);

    obs.subscribe({
      next: () => this.router.navigate(['/tasks']),
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.error?.message ?? 'حدث خطأ، يرجى المحاولة مرة أخرى');
      }
    });
  }
}
