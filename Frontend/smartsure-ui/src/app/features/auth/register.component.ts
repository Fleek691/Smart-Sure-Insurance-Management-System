import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { AuthStateService } from '../../core/services/auth-state.service';
import { SharedModule } from '../../shared/shared.module';

@Component({
  selector: 'app-register-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, SharedModule],
  template: `
    <section class="auth-page">
      <div class="hero-card">
        <p class="eyebrow">Registration</p>
        <h2>Create an account, then verify the OTP sent to your email.</h2>
        <p>
          The architecture requires OTP-based signup, so this flow stays in two stages: register first, verify immediately after.
        </p>
      </div>

      <form class="surface-card form-card" *ngIf="phase === 'register'" (ngSubmit)="register()">
        <div class="card-header">
          <div>
            <p class="eyebrow">Step 1 of 2</p>
            <h3>Create account</h3>
          </div>
          <app-stepper [steps]="['Register', 'Verify OTP']" [activeIndex]="0"></app-stepper>
        </div>

        <label><span>Full name</span><input name="fullName" [(ngModel)]="registerModel.fullName" required /></label>
        <label><span>Email</span><input name="email" [(ngModel)]="registerModel.email" type="email" required /></label>
        <label><span>Password</span><input name="password" [(ngModel)]="registerModel.password" type="password" required /></label>
        <label><span>Phone number</span><input name="phoneNumber" [(ngModel)]="registerModel.phoneNumber" /></label>
        <label><span>Address</span><input name="address" [(ngModel)]="registerModel.address" /></label>

        <p class="error" *ngIf="errorMessage">{{ errorMessage }}</p>
        <button type="submit" class="primary-button" [disabled]="loading">{{ loading ? 'Sending OTP...' : 'Register and send OTP' }}</button>
        <div class="form-links"><a routerLink="/auth/login">Back to login</a></div>
      </form>

      <form class="surface-card form-card" *ngIf="phase === 'otp'" (ngSubmit)="verifyOtp()">
        <div class="card-header">
          <div>
            <p class="eyebrow">Step 2 of 2</p>
            <h3>Verify OTP</h3>
          </div>
          <app-stepper [steps]="['Register', 'Verify OTP']" [activeIndex]="1"></app-stepper>
        </div>

        <label><span>Email</span><input name="otpEmail" [(ngModel)]="otpModel.email" type="email" required /></label>
        <label><span>OTP</span><input name="otp" [(ngModel)]="otpModel.otp" maxlength="8" required /></label>

        <p class="error" *ngIf="errorMessage">{{ errorMessage }}</p>
        <p class="hint" *ngIf="infoMessage">{{ infoMessage }}</p>

        <button type="submit" class="primary-button" [disabled]="loading">{{ loading ? 'Verifying...' : 'Verify OTP' }}</button>
        <button type="button" class="secondary-button" (click)="resendOtp()" [disabled]="loading">Resend OTP</button>
        <div class="form-links"><a (click)="restart()">Use a different email</a></div>
      </form>
    </section>
  `,
  styles: [`
    :host { display: block; }
    .auth-page { display: grid; grid-template-columns: 1.05fr 0.95fr; gap: 1rem; align-items: stretch; }
    .hero-card, .surface-card { border-radius: 24px; padding: 1.5rem; border: 1px solid rgba(84, 101, 255, 0.14); box-shadow: var(--shadow); background: rgba(255,255,255,0.82); }
    .hero-card { min-height: 28rem; background: linear-gradient(135deg, rgba(155,177,255,0.22), rgba(226,253,255,0.9)); }
    .hero-card h2 { margin: 0.5rem 0 0.75rem; font-family: 'Space Grotesk', sans-serif; font-size: clamp(2rem, 3vw, 3.2rem); color: var(--ink); }
    .hero-card p { margin: 0; max-width: 30rem; color: var(--muted); line-height: 1.7; }
    .form-card { display: grid; gap: 1rem; }
    .card-header { display: flex; justify-content: space-between; gap: 1rem; align-items: flex-start; }
    h3 { margin: 0.2rem 0 0; font-family: 'Space Grotesk', sans-serif; font-size: 1.7rem; color: var(--ink); }
    label { display: grid; gap: 0.42rem; }
    label span { font-weight: 700; color: var(--ink); }
    input { border: 1px solid var(--line); background: rgba(255,255,255,0.82); border-radius: 16px; padding: 0.9rem 1rem; color: var(--ink); }
    .primary-button, .secondary-button { border: 0; border-radius: 16px; padding: 0.95rem 1.1rem; font-weight: 700; cursor: pointer; }
    .primary-button { color: white; background: linear-gradient(135deg, #5465ff, #788bff); }
    .secondary-button { background: rgba(84,101,255,0.1); color: var(--primary); }
    .form-links { display: flex; justify-content: space-between; font-weight: 600; color: var(--primary); }
    .error { color: var(--danger); margin: 0; }
    .hint { color: var(--muted); margin: 0; }
    @media (max-width: 980px) { .auth-page { grid-template-columns: 1fr; } }
  `]
})
export class RegisterComponent {
  phase: 'register' | 'otp' = 'register';
  loading = false;
  errorMessage = '';
  infoMessage = '';

  registerModel = {
    fullName: '',
    email: '',
    password: '',
    phoneNumber: '',
    address: ''
  };

  otpModel = {
    email: '',
    otp: ''
  };

  constructor(
    private readonly authService: AuthService,
    private readonly authState: AuthStateService,
    private readonly router: Router
  ) {}

  register(): void {
    this.loading = true;
    this.errorMessage = '';
    this.infoMessage = '';

    this.authService.register(this.registerModel).subscribe({
      next: () => {
        this.otpModel.email = this.registerModel.email;
        this.phase = 'otp';
        this.infoMessage = 'OTP sent to your email. Enter it to activate your account.';
      },
      error: (error) => {
        this.errorMessage = this.resolveError(error);
        this.loading = false;
      },
      complete: () => {
        this.loading = false;
      }
    });
  }

  verifyOtp(): void {
    this.loading = true;
    this.errorMessage = '';

    this.authService.verifyRegistrationOtp(this.otpModel).subscribe({
      next: (session) => {
        this.authState.setSession(session);
        void this.router.navigateByUrl(this.authState.primaryRoute());
      },
      error: (error) => {
        this.errorMessage = this.resolveError(error);
        this.loading = false;
      },
      complete: () => {
        this.loading = false;
      }
    });
  }

  resendOtp(): void {
    this.loading = true;
    this.authService.resendRegistrationOtp({ email: this.otpModel.email }).subscribe({
      next: () => {
        this.infoMessage = 'OTP resent.';
      },
      error: (error) => {
        this.errorMessage = this.resolveError(error);
      },
      complete: () => {
        this.loading = false;
      }
    });
  }

  restart(): void {
    this.phase = 'register';
    this.errorMessage = '';
    this.infoMessage = '';
  }

  private resolveError(error: unknown): string {
    const response = error as { error?: { title?: string; detail?: string }; message?: string };
    return response?.error?.detail ?? response?.error?.title ?? response?.message ?? 'Unable to register.';
  }
}
