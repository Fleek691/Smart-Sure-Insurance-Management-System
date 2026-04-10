import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-recovery-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <section class="auth-page">
      <div class="hero-card">
        <p class="eyebrow">Recovery</p>
        <h2>Reset access without leaving the gateway flow.</h2>
        <p>
          The backend exposes separate forgot-password and reset-password endpoints. This screen keeps both in one place for convenience.
        </p>
      </div>

      <div class="stack">
        <form class="surface-card form-card" (ngSubmit)="requestReset()">
          <div class="card-header">
            <div>
              <p class="eyebrow">Request token</p>
              <h3>Forgot password</h3>
            </div>
          </div>

          <label><span>Email</span><input name="forgotEmail" [(ngModel)]="forgotModel.email" type="email" required /></label>
          <p class="error" *ngIf="errorMessage">{{ errorMessage }}</p>
          <p class="hint" *ngIf="infoMessage">{{ infoMessage }}</p>
          <button type="submit" class="primary-button" [disabled]="loading">Request reset</button>
        </form>

        <form class="surface-card form-card" (ngSubmit)="resetPassword()">
          <div class="card-header">
            <div>
              <p class="eyebrow">Complete reset</p>
              <h3>Reset password</h3>
            </div>
          </div>

          <label><span>Email</span><input name="resetEmail" [(ngModel)]="resetModel.email" type="email" required /></label>
          <label><span>Token</span><input name="token" [(ngModel)]="resetModel.token" required /></label>
          <label><span>New password</span><input name="newPassword" [(ngModel)]="resetModel.newPassword" type="password" required /></label>

          <button type="submit" class="primary-button" [disabled]="loading">Reset password</button>
          <div class="form-links"><a routerLink="/auth/login">Back to login</a></div>
        </form>
      </div>
    </section>
  `,
  styles: [`
    :host { display: block; }
    .auth-page { display: grid; grid-template-columns: 1.05fr 0.95fr; gap: 1rem; align-items: stretch; }
    .stack { display: grid; gap: 1rem; }
    .hero-card, .surface-card { border-radius: 24px; padding: 1.5rem; border: 1px solid rgba(84, 101, 255, 0.14); box-shadow: var(--shadow); background: rgba(255,255,255,0.82); }
    .hero-card { min-height: 28rem; background: linear-gradient(135deg, rgba(226,253,255,0.92), rgba(191,215,255,0.74)); }
    .hero-card h2 { margin: 0.5rem 0 0.75rem; font-family: 'Space Grotesk', sans-serif; font-size: clamp(2rem, 3vw, 3.2rem); color: var(--ink); }
    .hero-card p { margin: 0; max-width: 30rem; color: var(--muted); line-height: 1.7; }
    .form-card { display: grid; gap: 1rem; }
    .card-header { display: flex; justify-content: space-between; gap: 1rem; align-items: flex-start; }
    h3 { margin: 0.2rem 0 0; font-family: 'Space Grotesk', sans-serif; font-size: 1.7rem; color: var(--ink); }
    label { display: grid; gap: 0.42rem; }
    label span { font-weight: 700; color: var(--ink); }
    input { border: 1px solid var(--line); background: rgba(255,255,255,0.82); border-radius: 16px; padding: 0.9rem 1rem; color: var(--ink); }
    .primary-button { border: 0; border-radius: 16px; padding: 0.95rem 1.1rem; font-weight: 700; cursor: pointer; color: white; background: linear-gradient(135deg, #5465ff, #788bff); }
    .form-links { display: flex; justify-content: flex-end; font-weight: 600; color: var(--primary); }
    .error { color: var(--danger); margin: 0; }
    .hint { color: var(--muted); margin: 0; }
    @media (max-width: 980px) { .auth-page { grid-template-columns: 1fr; } }
  `]
})
export class RecoveryComponent {
  loading = false;
  errorMessage = '';
  infoMessage = '';

  forgotModel = { email: '' };
  resetModel = { email: '', token: '', newPassword: '' };

  constructor(private readonly authService: AuthService) {}

  requestReset(): void {
    this.loading = true;
    this.errorMessage = '';
    this.authService.requestPasswordReset(this.forgotModel).subscribe({
      next: () => {
        this.infoMessage = 'If the account exists, a reset token has been sent.';
      },
      error: (error) => {
        this.errorMessage = this.resolveError(error);
      },
      complete: () => {
        this.loading = false;
      }
    });
  }

  resetPassword(): void {
    this.loading = true;
    this.errorMessage = '';
    this.authService.resetPassword(this.resetModel).subscribe({
      next: () => {
        this.infoMessage = 'Password updated. You can sign in again now.';
      },
      error: (error) => {
        this.errorMessage = this.resolveError(error);
      },
      complete: () => {
        this.loading = false;
      }
    });
  }

  private resolveError(error: unknown): string {
    const response = error as { error?: { title?: string; detail?: string }; message?: string };
    return response?.error?.detail ?? response?.error?.title ?? response?.message ?? 'Unable to complete recovery action.';
  }
}
