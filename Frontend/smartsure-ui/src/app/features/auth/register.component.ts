import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { AuthStateService } from '../../core/services/auth-state.service';
import { SharedModule } from '../../shared/shared.module';
import { LogoComponent } from '../../shared/components/logo.component';

@Component({
  selector: 'app-register-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, SharedModule, LogoComponent],
  template: `
    <div class="register-shell">

      <!-- LEFT PANEL — branding + illustration -->
      <div class="left-panel">
        <div class="brand">
          <app-logo [size]="42"></app-logo>
          <span class="brand-name">SmartSure</span>
        </div>

        <div class="illustration-area">
          <!-- Registration illustration -->
          <svg viewBox="0 0 320 280" fill="none" xmlns="http://www.w3.org/2000/svg" class="hero-illus">
            <defs>
              <filter id="rs" x="-10%" y="-10%" width="120%" height="120%">
                <feDropShadow dx="0" dy="4" stdDeviation="8" flood-color="#5465FF" flood-opacity="0.1"/>
              </filter>
            </defs>
            <!-- Background circles -->
            <circle cx="160" cy="140" r="110" fill="#f0f4ff"/>
            <circle cx="160" cy="140" r="80" fill="#f7fbff"/>

            <!-- Form card -->
            <rect x="95" y="50" width="150" height="190" rx="16" fill="white" stroke="#5465FF" stroke-width="2" filter="url(#rs)"/>
            <!-- Header bar -->
            <rect x="95" y="50" width="150" height="36" rx="16" fill="#eef2ff"/>
            <rect x="95" y="70" width="150" height="16" fill="#eef2ff"/>
            <!-- Avatar circle -->
            <circle cx="170" cy="62" r="14" fill="white" stroke="#5465FF" stroke-width="2"/>
            <circle cx="170" cy="58" r="5" fill="#5465FF" fill-opacity="0.3"/>
            <path d="M160 68 Q165 73 170 73 Q175 73 180 68" stroke="#5465FF" stroke-width="1.5" fill="none" stroke-linecap="round"/>

            <!-- Form fields -->
            <rect x="110" y="96" width="120" height="14" rx="7" fill="#f0f4ff" stroke="#c7d2fe" stroke-width="1"/>
            <rect x="110" y="120" width="120" height="14" rx="7" fill="#f0f4ff" stroke="#c7d2fe" stroke-width="1"/>
            <rect x="110" y="144" width="120" height="14" rx="7" fill="#f0f4ff" stroke="#c7d2fe" stroke-width="1"/>
            <rect x="110" y="168" width="120" height="14" rx="7" fill="#f0f4ff" stroke="#c7d2fe" stroke-width="1"/>

            <!-- Field labels -->
            <rect x="114" y="100" width="30" height="5" rx="2.5" fill="#9BB1FF"/>
            <rect x="114" y="124" width="24" height="5" rx="2.5" fill="#9BB1FF"/>
            <rect x="114" y="148" width="36" height="5" rx="2.5" fill="#9BB1FF"/>
            <rect x="114" y="172" width="28" height="5" rx="2.5" fill="#9BB1FF"/>

            <!-- Submit button -->
            <rect x="110" y="198" width="120" height="20" rx="10" fill="#5465FF"/>
            <rect x="145" y="204" width="50" height="6" rx="3" fill="white" fill-opacity="0.7"/>

            <!-- Person on left -->
            <ellipse cx="60" cy="140" rx="16" ry="17" fill="white" stroke="#5465FF" stroke-width="2"/>
            <path d="M44 134 Q46 120 60 119 Q74 120 76 134 Q72 124 60 123 Q48 124 44 134Z" fill="#5465FF"/>
            <circle cx="55" cy="138" r="2.2" fill="#5465FF"/>
            <circle cx="65" cy="138" r="2.2" fill="#5465FF"/>
            <path d="M54 146 Q60 151 66 146" stroke="#5465FF" stroke-width="1.6" stroke-linecap="round" fill="none"/>
            <path d="M48 157 Q44 166 44 178 L76 178 Q76 166 72 157 Q66 151 60 151 Q54 151 48 157Z"
                  fill="white" stroke="#5465FF" stroke-width="2" stroke-linejoin="round"/>
            <!-- Arm pointing at form -->
            <path d="M76 162 Q88 156 95 145" stroke="#5465FF" stroke-width="2.5" stroke-linecap="round" fill="none"/>

            <!-- Checkmark floating -->
            <circle cx="265" cy="75" r="18" fill="#eef2ff" stroke="#5465FF" stroke-width="1.5"/>
            <path d="M256 75 L262 81 L275 68" stroke="#5465FF" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"/>

            <!-- Lock icon floating -->
            <circle cx="75" cy="80" r="14" fill="#eef2ff" stroke="#9BB1FF" stroke-width="1.5"/>
            <rect x="69" y="82" width="12" height="9" rx="2" fill="none" stroke="#5465FF" stroke-width="1.5"/>
            <path d="M72 82V79a3 3 0 016 0v3" fill="none" stroke="#5465FF" stroke-width="1.5" stroke-linecap="round"/>

            <!-- Sparkles -->
            <path d="M280 170 L282 164 L284 170 L290 172 L284 174 L282 180 L280 174 L274 172Z" fill="#5465FF" fill-opacity="0.4"/>
            <circle cx="40" cy="210" r="3" fill="#9BB1FF" fill-opacity="0.6"/>
            <circle cx="290" cy="230" r="2.5" fill="#BFD7FF"/>
          </svg>
        </div>

        <div class="left-footer">
          <p class="tagline">Join thousands who trust SmartSure<br>to protect what matters most.</p>
          <div class="trust-pills">
            <span>
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none"><path d="M12 2L3 7v6c0 5.25 4.05 10.15 9 11.25C17.95 23.15 22 18.25 22 13V7L12 2Z" stroke="#5465FF" stroke-width="2" stroke-linejoin="round"/></svg>
              Secure & encrypted
            </span>
            <span>
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none"><circle cx="12" cy="12" r="10" stroke="#5465FF" stroke-width="2"/><path d="M12 6v6l4 2" stroke="#5465FF" stroke-width="2" stroke-linecap="round"/></svg>
              2 min signup
            </span>
            <span>
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none"><path d="M9 12l2 2 4-4" stroke="#5465FF" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/><path d="M12 2L3 7v6c0 5.25 4.05 10.15 9 11.25C17.95 23.15 22 18.25 22 13V7L12 2Z" stroke="#5465FF" stroke-width="2" stroke-linejoin="round"/></svg>
              No hidden fees
            </span>
          </div>
        </div>
      </div>

      <!-- RIGHT PANEL — form -->
      <div class="right-panel">
        <div class="form-wrapper">

          <!-- ── STEP 1: REGISTER ── -->
          <ng-container *ngIf="phase === 'register'">
            <div class="form-header">
              <h2>Create your account</h2>
              <p>Fill in your details to get started with SmartSure</p>
            </div>

            <div class="step-indicator">
              <div class="step active"><span>1</span> Details</div>
              <div class="step-line"></div>
              <div class="step"><span>2</span> Verify</div>
            </div>

            <form (ngSubmit)="register()">
              <div class="field">
                <label for="fullName">Full name</label>
                <div class="input-wrap">
                  <svg class="input-icon" width="18" height="18" viewBox="0 0 24 24" fill="none">
                    <path d="M20 21v-2a4 4 0 00-4-4H8a4 4 0 00-4 4v2" stroke="#9BB1FF" stroke-width="2" stroke-linecap="round"/>
                    <circle cx="12" cy="7" r="4" stroke="#9BB1FF" stroke-width="2"/>
                  </svg>
                  <input id="fullName" name="fullName" [(ngModel)]="registerModel.fullName" placeholder="John Doe" required />
                </div>
              </div>

              <div class="field">
                <label for="regEmail">Email address</label>
                <div class="input-wrap">
                  <svg class="input-icon" width="18" height="18" viewBox="0 0 24 24" fill="none">
                    <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z" stroke="#9BB1FF" stroke-width="2" stroke-linecap="round"/>
                    <polyline points="22,6 12,13 2,6" stroke="#9BB1FF" stroke-width="2" stroke-linecap="round"/>
                  </svg>
                  <input id="regEmail" name="email" [(ngModel)]="registerModel.email" type="email" placeholder="you@example.com" required autocomplete="email"/>
                </div>
              </div>

              <div class="field">
                <label for="regPassword">Password</label>
                <div class="input-wrap">
                  <svg class="input-icon" width="18" height="18" viewBox="0 0 24 24" fill="none">
                    <rect x="3" y="11" width="18" height="11" rx="2" stroke="#9BB1FF" stroke-width="2"/>
                    <path d="M7 11V7a5 5 0 0110 0v4" stroke="#9BB1FF" stroke-width="2" stroke-linecap="round"/>
                  </svg>
                  <input id="regPassword" name="password" [(ngModel)]="registerModel.password" type="password" placeholder="At least 8 characters" required autocomplete="new-password"/>
                </div>
              </div>

              <div class="field-row">
                <div class="field">
                  <label for="phone">Phone number</label>
                  <div class="input-wrap">
                    <svg class="input-icon" width="18" height="18" viewBox="0 0 24 24" fill="none">
                      <path d="M22 16.92v3a2 2 0 01-2.18 2 19.79 19.79 0 01-8.63-3.07 19.5 19.5 0 01-6-6A19.79 19.79 0 012.12 4.18 2 2 0 014.11 2h3a2 2 0 012 1.72c.127.96.362 1.903.7 2.81a2 2 0 01-.45 2.11L8.09 9.91a16 16 0 006 6l1.27-1.27a2 2 0 012.11-.45c.907.338 1.85.573 2.81.7A2 2 0 0122 16.92z" stroke="#9BB1FF" stroke-width="2"/>
                    </svg>
                    <input id="phone" name="phoneNumber" [(ngModel)]="registerModel.phoneNumber" placeholder="+91 98765 43210"/>
                  </div>
                </div>

                <div class="field">
                  <label for="address">Address</label>
                  <div class="input-wrap">
                    <svg class="input-icon" width="18" height="18" viewBox="0 0 24 24" fill="none">
                      <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0118 0z" stroke="#9BB1FF" stroke-width="2"/>
                      <circle cx="12" cy="10" r="3" stroke="#9BB1FF" stroke-width="2"/>
                    </svg>
                    <input id="address" name="address" [(ngModel)]="registerModel.address" placeholder="City, State"/>
                  </div>
                </div>
              </div>

              <p class="error-msg" *ngIf="errorMessage">{{ errorMessage }}</p>

              <button type="submit" class="btn-primary" [disabled]="loading">
                <span *ngIf="!loading">Create account & send OTP</span>
                <span *ngIf="loading" class="spinner-wrap">
                  <svg class="spinner" viewBox="0 0 24 24"><circle cx="12" cy="12" r="10" stroke="white" stroke-width="3" fill="none" stroke-dasharray="31.4" stroke-dashoffset="10"/></svg>
                  Creating account...
                </span>
              </button>

              <p class="login-prompt">
                Already have an account? <a routerLink="/auth/login">Sign in</a>
              </p>
            </form>
          </ng-container>

          <!-- ── STEP 2: VERIFY OTP ── -->
          <ng-container *ngIf="phase === 'otp'">
            <div class="form-header">
              <h2>Verify your email</h2>
              <p>We sent a one-time code to <strong>{{ otpModel.email }}</strong></p>
            </div>

            <div class="step-indicator">
              <div class="step completed"><svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="3"><polyline points="20 6 9 17 4 12"/></svg> Details</div>
              <div class="step-line active"></div>
              <div class="step active"><span>2</span> Verify</div>
            </div>

            <form (ngSubmit)="verifyOtp()">
              <div class="field">
                <label for="otpCode">Verification code</label>
                <div class="input-wrap">
                  <svg class="input-icon" width="18" height="18" viewBox="0 0 24 24" fill="none">
                    <rect x="3" y="11" width="18" height="11" rx="2" stroke="#9BB1FF" stroke-width="2"/>
                    <path d="M7 11V7a5 5 0 0110 0v4" stroke="#9BB1FF" stroke-width="2" stroke-linecap="round"/>
                  </svg>
                  <input id="otpCode" name="otp" [(ngModel)]="otpModel.otp" maxlength="8" placeholder="Enter OTP from email" required autocomplete="one-time-code"/>
                </div>
              </div>

              <p class="error-msg" *ngIf="errorMessage">{{ errorMessage }}</p>
              <p class="info-msg" *ngIf="infoMessage">{{ infoMessage }}</p>

              <button type="submit" class="btn-primary" [disabled]="loading">
                <span *ngIf="!loading">Verify & continue</span>
                <span *ngIf="loading" class="spinner-wrap">
                  <svg class="spinner" viewBox="0 0 24 24"><circle cx="12" cy="12" r="10" stroke="white" stroke-width="3" fill="none" stroke-dasharray="31.4" stroke-dashoffset="10"/></svg>
                  Verifying...
                </span>
              </button>

              <div class="otp-actions">
                <button type="button" class="btn-ghost" (click)="resendOtp()" [disabled]="loading">Resend OTP</button>
                <button type="button" class="btn-ghost" (click)="restart()">Use different email</button>
              </div>
            </form>
          </ng-container>

        </div>
      </div>
    </div>
  `,
  styles: [`
    :host { display: block; min-height: 100vh; }

    .register-shell {
      display: grid;
      grid-template-columns: 1fr 1fr;
      min-height: 100vh;
    }

    /* ── LEFT PANEL ── */
    .left-panel {
      background: #ffffff;
      display: flex;
      flex-direction: column;
      padding: 2.5rem 2.5rem 2rem;
      position: relative;
      overflow: hidden;
      border-right: 1px solid #f0f4ff;
    }
    .left-panel::before {
      content: '';
      position: absolute;
      top: -80px; right: -80px;
      width: 320px; height: 320px;
      border-radius: 50%;
      background: radial-gradient(circle, #eef2ff 0%, transparent 70%);
      pointer-events: none;
    }
    .left-panel::after {
      content: '';
      position: absolute;
      bottom: -60px; left: -60px;
      width: 260px; height: 260px;
      border-radius: 50%;
      background: radial-gradient(circle, #f0f8ff 0%, transparent 70%);
      pointer-events: none;
    }

    .brand {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      position: relative;
      z-index: 1;
    }
    .brand-icon {
      width: 42px; height: 42px;
      background: #eef2ff;
      border-radius: 12px;
      display: flex; align-items: center; justify-content: center;
      border: 1px solid #c7d2fe;
    }
    .brand-name {
      font-family: 'Space Grotesk', sans-serif;
      font-size: 1.5rem;
      font-weight: 700;
      color: #14213d;
      letter-spacing: -0.02em;
    }

    .illustration-area {
      flex: 1;
      display: flex;
      align-items: center;
      justify-content: center;
      position: relative;
      z-index: 1;
      padding: 1rem 0;
    }
    .hero-illus {
      width: 85%;
      max-width: 380px;
      height: auto;
    }

    .left-footer {
      position: relative;
      z-index: 1;
    }
    .tagline {
      color: #14213d;
      font-family: 'Space Grotesk', sans-serif;
      font-size: 1.15rem;
      font-weight: 600;
      margin: 0 0 1rem;
      line-height: 1.55;
      letter-spacing: -0.01em;
    }
    .trust-pills {
      display: flex;
      flex-wrap: wrap;
      gap: 0.5rem;
    }
    .trust-pills span {
      display: inline-flex;
      align-items: center;
      gap: 0.4rem;
      background: #f5f7ff;
      border: 1px solid #c7d2fe;
      border-radius: 999px;
      padding: 0.38rem 0.85rem;
      font-size: 0.8rem;
      font-weight: 600;
      color: #3730a3;
    }

    /* ── RIGHT PANEL ── */
    .right-panel {
      background: #ffffff;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 2rem;
      overflow-y: auto;
    }

    .form-wrapper {
      width: 100%;
      max-width: 420px;
    }

    .form-header {
      margin-bottom: 1.5rem;
    }
    .form-header h2 {
      font-family: 'Space Grotesk', sans-serif;
      font-size: 1.9rem;
      font-weight: 700;
      color: #14213d;
      margin: 0 0 0.35rem;
      letter-spacing: -0.03em;
    }
    .form-header p {
      color: #5f6b82;
      margin: 0;
      font-size: 0.95rem;
    }

    /* ── Step indicator ── */
    .step-indicator {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      margin-bottom: 1.5rem;
    }
    .step {
      display: flex;
      align-items: center;
      gap: 0.4rem;
      font-size: 0.82rem;
      font-weight: 600;
      color: #9BB1FF;
    }
    .step span {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 24px; height: 24px;
      border-radius: 50%;
      background: #f0f4ff;
      border: 1.5px solid #c7d2fe;
      font-size: 0.75rem;
      font-weight: 700;
      color: #5465FF;
    }
    .step.active { color: #14213d; }
    .step.active span { background: #5465FF; color: white; border-color: #5465FF; }
    .step.completed { color: #14213d; }
    .step.completed svg {
      display: inline-flex;
      width: 24px; height: 24px;
      background: #10B981;
      border-radius: 50%;
      padding: 5px;
    }
    .step-line {
      flex: 1;
      height: 2px;
      background: #E2FDFF;
    }
    .step-line.active {
      background: linear-gradient(90deg, #10B981, #5465FF);
    }

    /* ── Form fields ── */
    .field {
      display: flex;
      flex-direction: column;
      gap: 0.4rem;
      margin-bottom: 0.9rem;
    }
    .field label {
      font-weight: 600;
      font-size: 0.88rem;
      color: #14213d;
    }
    .field-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 0.75rem;
    }
    .input-wrap {
      position: relative;
    }
    .input-icon {
      position: absolute;
      left: 0.9rem;
      top: 50%;
      transform: translateY(-50%);
      pointer-events: none;
    }
    .input-wrap input {
      width: 100%;
      padding: 0.8rem 1rem 0.8rem 2.6rem;
      border: 1.5px solid #E2FDFF;
      border-radius: 14px;
      background: #f7fbff;
      color: #14213d;
      font-size: 0.95rem;
      transition: border-color 0.2s, box-shadow 0.2s;
    }
    .input-wrap input:focus {
      outline: none;
      border-color: #788BFF;
      background: white;
      box-shadow: 0 0 0 4px rgba(120,139,255,0.12);
    }
    .input-wrap input::placeholder { color: #9BB1FF; }

    .btn-primary {
      width: 100%;
      padding: 0.9rem;
      border: none;
      border-radius: 14px;
      background: linear-gradient(135deg, #5465FF 0%, #788BFF 100%);
      color: white;
      font-size: 0.98rem;
      font-weight: 700;
      cursor: pointer;
      box-shadow: 0 8px 24px rgba(84,101,255,0.35);
      transition: transform 0.15s, box-shadow 0.15s;
      margin-bottom: 1rem;
      margin-top: 0.5rem;
    }
    .btn-primary:hover:not(:disabled) {
      transform: translateY(-1px);
      box-shadow: 0 12px 32px rgba(84,101,255,0.45);
    }
    .btn-primary:disabled { opacity: 0.7; cursor: not-allowed; }

    .spinner-wrap { display: flex; align-items: center; justify-content: center; gap: 0.5rem; }
    .spinner { width: 18px; height: 18px; animation: spin 0.8s linear infinite; }
    @keyframes spin { to { transform: rotate(360deg); } }

    .login-prompt {
      text-align: center;
      font-size: 0.9rem;
      color: #5f6b82;
      margin: 0;
    }
    .login-prompt a {
      color: #5465FF;
      font-weight: 700;
      text-decoration: none;
    }
    .login-prompt a:hover { text-decoration: underline; }

    .otp-actions {
      display: flex;
      justify-content: center;
      gap: 1.5rem;
    }
    .btn-ghost {
      background: none;
      border: 0;
      color: #5465FF;
      font-weight: 600;
      font-size: 0.88rem;
      cursor: pointer;
      padding: 0;
    }
    .btn-ghost:hover { text-decoration: underline; }
    .btn-ghost:disabled { opacity: 0.5; cursor: not-allowed; }

    .error-msg { color: #d1495b; font-size: 0.88rem; margin: 0 0 0.5rem; }
    .info-msg  { color: #5f6b82;  font-size: 0.88rem; margin: 0 0 0.5rem; }

    /* ── RESPONSIVE ── */
    @media (max-width: 860px) {
      .register-shell { grid-template-columns: 1fr; }
      .left-panel { padding: 2rem 1.5rem 1.5rem; border-right: none; border-bottom: 1px solid #f0f4ff; }
      .illustration-area { padding: 0.5rem 0; }
      .hero-illus { width: 65%; }
    }
    @media (max-width: 480px) {
      .right-panel { padding: 1.5rem 1.25rem; }
      .form-header h2 { font-size: 1.5rem; }
      .field-row { grid-template-columns: 1fr; }
    }
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
    const r = error as {
      error?: {
        detail?: string;
        title?: string;
        errors?: Record<string, string[]>;
      };
      message?: string;
    };

    // Our middleware / InvalidModelStateResponseFactory returns { detail }
    if (r?.error?.detail) return r.error.detail;

    // Fallback: ASP.NET default validation shape { errors: { Field: ["msg"] } }
    if (r?.error?.errors) {
      const msgs = Object.values(r.error.errors).flat();
      if (msgs.length) return msgs.join(' ');
    }

    return r?.error?.title ?? r?.message ?? 'Unable to register.';
  }
}
