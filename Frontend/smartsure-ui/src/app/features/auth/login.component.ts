import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { AuthStateService } from '../../core/services/auth-state.service';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="login-shell">

      <!-- LEFT PANEL — illustration + branding -->
      <div class="left-panel">
        <div class="brand">
          <div class="brand-icon">
            <svg width="28" height="28" viewBox="0 0 28 28" fill="none">
              <path d="M14 2L3 7.5V14c0 6.075 4.667 11.75 11 13 6.333-1.25 11-6.925 11-13V7.5L14 2Z" fill="#5465FF"/>
              <path d="M9 14l3.5 3.5L19 10" stroke="white" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"/>
            </svg>
          </div>
          <span class="brand-name">SmartSure</span>
        </div>

        <div class="illustration-area">
          <!-- Three illustrations in a row -->
          <div class="illus-grid">

            <!-- ① SHIELD / PROTECTION -->
            <div class="illus-card">
              <svg viewBox="0 0 160 200" fill="none" xmlns="http://www.w3.org/2000/svg">
                <defs>
                  <filter id="s1" x="-20%" y="-20%" width="140%" height="140%">
                    <feDropShadow dx="0" dy="4" stdDeviation="6" flood-color="#5465FF" flood-opacity="0.1"/>
                  </filter>
                </defs>
                <!-- subtle bg circle -->
                <circle cx="80" cy="95" r="68" fill="#f0f4ff"/>

                <!-- Shield -->
                <path d="M80 28 L44 46 L44 92 C44 118 60 138 80 148 C100 138 116 118 116 92 L116 46 Z"
                      fill="white" stroke="#5465FF" stroke-width="2.5" stroke-linejoin="round" filter="url(#s1)"/>
                <!-- Shield inner -->
                <path d="M80 36 L52 51 L52 92 C52 114 64 132 80 140 C96 132 108 114 108 92 L108 51 Z"
                      fill="#eef2ff" stroke="#9BB1FF" stroke-width="1" stroke-linejoin="round"/>
                <!-- Checkmark -->
                <path d="M62 90 L74 104 L100 76" stroke="#5465FF" stroke-width="4" stroke-linecap="round" stroke-linejoin="round"/>

                <!-- Person standing below shield -->
                <!-- legs -->
                <rect x="72" y="162" width="8" height="22" rx="4" fill="white" stroke="#5465FF" stroke-width="2"/>
                <rect x="84" y="162" width="8" height="22" rx="4" fill="white" stroke="#5465FF" stroke-width="2"/>
                <!-- shoes -->
                <path d="M70 182 Q68 188 78 188 Q82 188 80 182Z" fill="#5465FF"/>
                <path d="M82 182 Q80 188 90 188 Q94 188 92 182Z" fill="#5465FF"/>
                <!-- body -->
                <path d="M64 148 Q60 155 60 168 L100 168 Q100 155 96 148 Q88 142 80 142 Q72 142 64 148Z"
                      fill="white" stroke="#5465FF" stroke-width="2" stroke-linejoin="round"/>
                <!-- neck -->
                <rect x="76" y="134" width="8" height="10" rx="4" fill="white" stroke="#5465FF" stroke-width="1.5"/>
                <!-- head -->
                <ellipse cx="80" cy="126" rx="14" ry="15" fill="white" stroke="#5465FF" stroke-width="2"/>
                <!-- hair -->
                <path d="M66 122 Q68 108 80 107 Q92 108 94 122 Q90 112 80 111 Q70 112 66 122Z" fill="#5465FF"/>
                <!-- eyes -->
                <circle cx="75" cy="124" r="2.5" fill="#5465FF"/>
                <circle cx="85" cy="124" r="2.5" fill="#5465FF"/>
                <!-- smile -->
                <path d="M74 131 Q80 136 86 131" stroke="#5465FF" stroke-width="1.8" stroke-linecap="round" fill="none"/>
                <!-- arms up -->
                <path d="M64 152 Q52 144 48 132" stroke="#5465FF" stroke-width="3" stroke-linecap="round" fill="none"/>
                <path d="M96 152 Q108 144 112 132" stroke="#5465FF" stroke-width="3" stroke-linecap="round" fill="none"/>

                <!-- sparkles -->
                <path d="M130 50 L132 44 L134 50 L140 52 L134 54 L132 60 L130 54 L124 52Z" fill="#5465FF" fill-opacity="0.5"/>
                <circle cx="22" cy="70" r="3" fill="#9BB1FF" fill-opacity="0.7"/>
                <circle cx="138" cy="130" r="2.5" fill="#BFD7FF"/>
              </svg>
              <p class="illus-label">Protection</p>
            </div>

            <!-- ② HOME / FAMILY -->
            <div class="illus-card illus-card--center">
              <svg viewBox="0 0 160 200" fill="none" xmlns="http://www.w3.org/2000/svg">
                <defs>
                  <filter id="s2" x="-20%" y="-20%" width="140%" height="140%">
                    <feDropShadow dx="0" dy="4" stdDeviation="6" flood-color="#5465FF" flood-opacity="0.1"/>
                  </filter>
                </defs>
                <circle cx="80" cy="95" r="68" fill="#f0f4ff"/>

                <!-- House -->
                <!-- roof -->
                <path d="M80 30 L28 72 L132 72 Z" fill="white" stroke="#5465FF" stroke-width="2.5" stroke-linejoin="round" filter="url(#s2)"/>
                <!-- chimney -->
                <rect x="100" y="42" width="10" height="20" rx="2" fill="white" stroke="#5465FF" stroke-width="2"/>
                <!-- smoke puffs -->
                <circle cx="105" cy="38" r="4" fill="white" stroke="#9BB1FF" stroke-width="1.5"/>
                <circle cx="109" cy="32" r="3" fill="white" stroke="#9BB1FF" stroke-width="1.5"/>
                <!-- walls -->
                <rect x="36" y="70" width="88" height="62" rx="4" fill="white" stroke="#5465FF" stroke-width="2.5"/>
                <!-- door -->
                <rect x="68" y="100" width="24" height="32" rx="4" fill="#eef2ff" stroke="#5465FF" stroke-width="2"/>
                <circle cx="88" cy="116" r="2" fill="#5465FF"/>
                <!-- windows -->
                <rect x="42" y="80" width="20" height="18" rx="3" fill="#eef2ff" stroke="#5465FF" stroke-width="1.8"/>
                <line x1="52" y1="80" x2="52" y2="98" stroke="#9BB1FF" stroke-width="1.2"/>
                <line x1="42" y1="89" x2="62" y2="89" stroke="#9BB1FF" stroke-width="1.2"/>
                <rect x="98" y="80" width="20" height="18" rx="3" fill="#eef2ff" stroke="#5465FF" stroke-width="1.8"/>
                <line x1="108" y1="80" x2="108" y2="98" stroke="#9BB1FF" stroke-width="1.2"/>
                <line x1="98" y1="89" x2="118" y2="89" stroke="#9BB1FF" stroke-width="1.2"/>
                <!-- path -->
                <path d="M68 132 L60 155 L100 155 L92 132Z" fill="#eef2ff" stroke="#9BB1FF" stroke-width="1.2"/>

                <!-- family — 3 figures -->
                <!-- child left -->
                <circle cx="46" cy="148" r="7" fill="white" stroke="#5465FF" stroke-width="1.8"/>
                <path d="M40 155 Q38 165 40 170 L52 170 Q54 165 52 155Z" fill="white" stroke="#5465FF" stroke-width="1.8" stroke-linejoin="round"/>
                <!-- adult center -->
                <circle cx="80" cy="144" r="9" fill="white" stroke="#5465FF" stroke-width="2"/>
                <path d="M70 153 Q68 166 70 172 L90 172 Q92 166 90 153Z" fill="white" stroke="#5465FF" stroke-width="2" stroke-linejoin="round"/>
                <!-- child right -->
                <circle cx="114" cy="148" r="7" fill="white" stroke="#5465FF" stroke-width="1.8"/>
                <path d="M108 155 Q106 165 108 170 L120 170 Q122 165 120 155Z" fill="white" stroke="#5465FF" stroke-width="1.8" stroke-linejoin="round"/>
                <!-- heart above -->
                <path d="M76 136 Q76 131 80 133 Q84 131 84 136 Q84 141 80 145 Q76 141 76 136Z" fill="#5465FF" fill-opacity="0.6"/>

                <!-- sparkles -->
                <path d="M20 50 L22 44 L24 50 L30 52 L24 54 L22 60 L20 54 L14 52Z" fill="#5465FF" fill-opacity="0.45"/>
                <circle cx="140" cy="60" r="3" fill="#9BB1FF" fill-opacity="0.7"/>
              </svg>
              <p class="illus-label">Home & Family</p>
            </div>

            <!-- ③ CLAIMS / DOCUMENT -->
            <div class="illus-card">
              <svg viewBox="0 0 160 200" fill="none" xmlns="http://www.w3.org/2000/svg">
                <defs>
                  <filter id="s3" x="-20%" y="-20%" width="140%" height="140%">
                    <feDropShadow dx="0" dy="4" stdDeviation="6" flood-color="#5465FF" flood-opacity="0.1"/>
                  </filter>
                </defs>
                <circle cx="80" cy="95" r="68" fill="#f0f4ff"/>

                <!-- Clipboard / document -->
                <rect x="34" y="44" width="92" height="116" rx="10" fill="white" stroke="#5465FF" stroke-width="2.5" filter="url(#s3)"/>
                <!-- clip top -->
                <rect x="60" y="36" width="40" height="18" rx="9" fill="white" stroke="#5465FF" stroke-width="2.5"/>
                <rect x="68" y="40" width="24" height="10" rx="5" fill="#eef2ff" stroke="#9BB1FF" stroke-width="1.2"/>
                <!-- header bar -->
                <rect x="34" y="56" width="92" height="18" rx="0" fill="#eef2ff"/>
                <rect x="34" y="56" width="92" height="4" rx="0" fill="#5465FF" fill-opacity="0.15"/>
                <!-- title line -->
                <rect x="44" y="62" width="52" height="5" rx="2.5" fill="#5465FF" fill-opacity="0.5"/>
                <!-- content rows -->
                <rect x="44" y="84" width="72" height="4" rx="2" fill="#BFD7FF"/>
                <rect x="44" y="94" width="60" height="4" rx="2" fill="#BFD7FF"/>
                <rect x="44" y="104" width="72" height="4" rx="2" fill="#BFD7FF"/>
                <rect x="44" y="114" width="48" height="4" rx="2" fill="#BFD7FF"/>
                <!-- divider -->
                <line x1="44" y1="126" x2="116" y2="126" stroke="#E2FDFF" stroke-width="1.5"/>
                <!-- approved stamp -->
                <rect x="44" y="132" width="72" height="22" rx="11" fill="#eef2ff" stroke="#5465FF" stroke-width="1.8"/>
                <path d="M54 143 L59 148 L70 136" stroke="#5465FF" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"/>
                <rect x="76" y="138" width="32" height="4" rx="2" fill="#5465FF" fill-opacity="0.5"/>
                <rect x="76" y="146" width="22" height="3.5" rx="1.8" fill="#9BB1FF" fill-opacity="0.6"/>

                <!-- person holding clipboard -->
                <!-- arm -->
                <path d="M34 90 Q22 80 20 65" stroke="#5465FF" stroke-width="3" stroke-linecap="round" fill="none"/>
                <!-- hand -->
                <ellipse cx="20" cy="60" rx="6" ry="7" fill="white" stroke="#5465FF" stroke-width="1.8"/>
                <!-- head -->
                <ellipse cx="20" cy="44" rx="10" ry="11" fill="white" stroke="#5465FF" stroke-width="2"/>
                <!-- hair -->
                <path d="M10 40 Q11 30 20 29 Q29 30 30 40 Q27 32 20 31 Q13 32 10 40Z" fill="#5465FF"/>
                <!-- eyes -->
                <circle cx="16" cy="43" r="1.8" fill="#5465FF"/>
                <circle cx="24" cy="43" r="1.8" fill="#5465FF"/>
                <!-- smile -->
                <path d="M15 49 Q20 53 25 49" stroke="#5465FF" stroke-width="1.5" stroke-linecap="round" fill="none"/>
                <!-- body -->
                <path d="M10 55 Q8 62 8 72 L32 72 Q32 62 30 55 Q25 50 20 50 Q15 50 10 55Z"
                      fill="white" stroke="#5465FF" stroke-width="1.8" stroke-linejoin="round"/>

                <!-- sparkles -->
                <path d="M136 44 L138 38 L140 44 L146 46 L140 48 L138 54 L136 48 L130 46Z" fill="#5465FF" fill-opacity="0.45"/>
                <circle cx="140" cy="140" r="3" fill="#9BB1FF" fill-opacity="0.6"/>
                <circle cx="22" cy="130" r="2.5" fill="#BFD7FF"/>
              </svg>
              <p class="illus-label">Easy Claims</p>
            </div>

          </div>
        </div>
        <div class="left-footer">
          <p class="tagline">Protecting what matters most —<br>your family, home, and future.</p>
          <div class="trust-pills">
            <span>
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none"><path d="M12 2L3 7v6c0 5.25 4.05 10.15 9 11.25C17.95 23.15 22 18.25 22 13V7L12 2Z" stroke="#5465FF" stroke-width="2" stroke-linejoin="round"/></svg>
              Trusted by 50,000+ customers
            </span>
            <span>
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none"><circle cx="12" cy="12" r="10" stroke="#5465FF" stroke-width="2"/><path d="M12 6v6l4 2" stroke="#5465FF" stroke-width="2" stroke-linecap="round"/></svg>
              Claims settled in 48 hrs
            </span>
            <span>
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none"><path d="M9 12l2 2 4-4" stroke="#5465FF" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/><path d="M12 2L3 7v6c0 5.25 4.05 10.15 9 11.25C17.95 23.15 22 18.25 22 13V7L12 2Z" stroke="#5465FF" stroke-width="2" stroke-linejoin="round"/></svg>
              IRDAI Regulated
            </span>
          </div>
        </div>
      </div>

      <!-- RIGHT PANEL — form -->
      <div class="right-panel">
        <div class="form-wrapper">
          <div class="form-header">
            <h2>Welcome back</h2>
            <p>Sign in to your SmartSure account</p>
          </div>

          <form (ngSubmit)="login()">
            <div class="field">
              <label for="email">Email address</label>
              <div class="input-wrap">
                <svg class="input-icon" width="18" height="18" viewBox="0 0 24 24" fill="none">
                  <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z" stroke="#9BB1FF" stroke-width="2" stroke-linecap="round"/>
                  <polyline points="22,6 12,13 2,6" stroke="#9BB1FF" stroke-width="2" stroke-linecap="round"/>
                </svg>
                <input id="email" name="email" [(ngModel)]="model.email" type="email" placeholder="you@example.com" required autocomplete="email"/>
              </div>
            </div>

            <div class="field">
              <label for="password">Password</label>
              <div class="input-wrap">
                <svg class="input-icon" width="18" height="18" viewBox="0 0 24 24" fill="none">
                  <rect x="3" y="11" width="18" height="11" rx="2" stroke="#9BB1FF" stroke-width="2"/>
                  <path d="M7 11V7a5 5 0 0 1 10 0v4" stroke="#9BB1FF" stroke-width="2" stroke-linecap="round"/>
                </svg>
                <input id="password" name="password" [(ngModel)]="model.password" type="password" placeholder="Your password" required autocomplete="current-password"/>
              </div>
            </div>

            <div class="form-meta">
              <a routerLink="/auth/recovery" class="forgot-link">Forgot password?</a>
            </div>

            <p class="error-msg" *ngIf="errorMessage">{{ errorMessage }}</p>

            <button type="submit" class="btn-primary" [disabled]="loading">
              <span *ngIf="!loading">Sign in</span>
              <span *ngIf="loading" class="spinner-wrap">
                <svg class="spinner" viewBox="0 0 24 24"><circle cx="12" cy="12" r="10" stroke="white" stroke-width="3" fill="none" stroke-dasharray="31.4" stroke-dashoffset="10"/></svg>
                Signing in...
              </span>
            </button>

            <div class="divider"><span>or</span></div>

            <button type="button" class="btn-google" (click)="googleLogin()" [disabled]="loading">
              <svg width="20" height="20" viewBox="0 0 24 24">
                <path d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" fill="#4285F4"/>
                <path d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" fill="#34A853"/>
                <path d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l3.66-2.84z" fill="#FBBC05"/>
                <path d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z" fill="#EA4335"/>
              </svg>
              Continue with Google
            </button>

            <p class="signup-prompt">
              Don't have an account? <a routerLink="/auth/register">Create one</a>
            </p>
          </form>
        </div>
      </div>

    </div>
  `,
  styles: [`
    :host { display: block; min-height: 100vh; }

    .login-shell {
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
      padding: 1.5rem 0 1rem;
    }

    .illus-grid {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 1rem;
      width: 100%;
    }

    .illus-card {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 0.6rem;
    }
    .illus-card svg {
      width: 100%;
      height: auto;
      border-radius: 20px;
      transition: transform 0.25s ease;
    }
    .illus-card:hover svg {
      transform: translateY(-4px);
    }
    .illus-card--center svg {
      transform: scale(1.06);
    }
    .illus-card--center:hover svg {
      transform: scale(1.06) translateY(-4px);
    }
    .illus-label {
      font-size: 0.78rem;
      font-weight: 700;
      color: #5465FF;
      letter-spacing: 0.04em;
      text-transform: uppercase;
      margin: 0;
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
      padding: 2.5rem 2rem;
    }

    .form-wrapper {
      width: 100%;
      max-width: 400px;
    }

    .form-header {
      margin-bottom: 2rem;
    }
    .form-header h2 {
      font-family: 'Space Grotesk', sans-serif;
      font-size: 2rem;
      font-weight: 700;
      color: #14213d;
      margin: 0 0 0.4rem;
      letter-spacing: -0.03em;
    }
    .form-header p {
      color: #5f6b82;
      margin: 0;
      font-size: 0.97rem;
    }

    .field {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      margin-bottom: 1.1rem;
    }
    .field label {
      font-weight: 600;
      font-size: 0.9rem;
      color: #14213d;
    }
    .input-wrap {
      position: relative;
    }
    .input-icon {
      position: absolute;
      left: 1rem;
      top: 50%;
      transform: translateY(-50%);
      pointer-events: none;
    }
    .input-wrap input {
      width: 100%;
      padding: 0.85rem 1rem 0.85rem 2.8rem;
      border: 1.5px solid #E2FDFF;
      border-radius: 14px;
      background: #f7fbff;
      color: #14213d;
      font-size: 0.97rem;
      transition: border-color 0.2s, box-shadow 0.2s;
    }
    .input-wrap input:focus {
      outline: none;
      border-color: #788BFF;
      background: white;
      box-shadow: 0 0 0 4px rgba(120,139,255,0.12);
    }
    .input-wrap input::placeholder { color: #9BB1FF; }

    .form-meta {
      display: flex;
      justify-content: flex-end;
      margin-bottom: 1.4rem;
    }
    .forgot-link {
      font-size: 0.87rem;
      font-weight: 600;
      color: #5465FF;
      text-decoration: none;
    }
    .forgot-link:hover { text-decoration: underline; }

    .btn-primary {
      width: 100%;
      padding: 0.95rem;
      border: none;
      border-radius: 14px;
      background: linear-gradient(135deg, #5465FF 0%, #788BFF 100%);
      color: white;
      font-size: 1rem;
      font-weight: 700;
      cursor: pointer;
      box-shadow: 0 8px 24px rgba(84,101,255,0.35);
      transition: transform 0.15s, box-shadow 0.15s;
      margin-bottom: 1.2rem;
    }
    .btn-primary:hover:not(:disabled) {
      transform: translateY(-1px);
      box-shadow: 0 12px 32px rgba(84,101,255,0.45);
    }
    .btn-primary:disabled { opacity: 0.7; cursor: not-allowed; }

    .spinner-wrap { display: flex; align-items: center; justify-content: center; gap: 0.5rem; }
    .spinner { width: 18px; height: 18px; animation: spin 0.8s linear infinite; }
    @keyframes spin { to { transform: rotate(360deg); } }

    .divider {
      display: flex;
      align-items: center;
      gap: 1rem;
      margin-bottom: 1.2rem;
      color: #9BB1FF;
      font-size: 0.85rem;
    }
    .divider::before, .divider::after {
      content: '';
      flex: 1;
      height: 1px;
      background: #E2FDFF;
    }

    .btn-google {
      width: 100%;
      padding: 0.9rem;
      border: 1.5px solid #BFD7FF;
      border-radius: 14px;
      background: white;
      color: #14213d;
      font-size: 0.97rem;
      font-weight: 600;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 0.75rem;
      transition: border-color 0.2s, background 0.2s;
      margin-bottom: 1.5rem;
    }
    .btn-google:hover:not(:disabled) {
      border-color: #788BFF;
      background: #f7fbff;
    }
    .btn-google:disabled { opacity: 0.7; cursor: not-allowed; }

    .signup-prompt {
      text-align: center;
      font-size: 0.9rem;
      color: #5f6b82;
      margin: 0;
    }
    .signup-prompt a {
      color: #5465FF;
      font-weight: 700;
      text-decoration: none;
    }
    .signup-prompt a:hover { text-decoration: underline; }

    .error-msg { color: #d1495b; font-size: 0.88rem; margin: 0 0 0.8rem; }

    /* ── RESPONSIVE ── */
    @media (max-width: 860px) {
      .login-shell { grid-template-columns: 1fr; }
      .left-panel { padding: 2rem 1.5rem 1.5rem; border-right: none; border-bottom: 1px solid #f0f4ff; }
      .illustration-area { padding: 1rem 0 0.5rem; }
      .illus-grid { gap: 0.6rem; }
    }
    @media (max-width: 480px) {
      .right-panel { padding: 2rem 1.25rem; }
      .form-header h2 { font-size: 1.6rem; }
      .illus-grid { grid-template-columns: repeat(3, 1fr); gap: 0.4rem; }
    }
  `]
})
export class LoginComponent implements OnInit {
  model = { email: '', password: '' };
  loading = false;
  errorMessage = '';

  constructor(
    private readonly authState: AuthStateService,
    private readonly authService: AuthService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    if (this.authState.isAuthenticated) {
      void this.router.navigateByUrl(this.authState.primaryRoute());
    }
  }

  login(): void {
    this.errorMessage = '';
    this.loading = true;

    this.authService.login(this.model).subscribe({
      next: (session) => {
        this.authState.setSession(session);
        void this.router.navigateByUrl(this.authState.primaryRoute());
      },
      error: (error) => {
        this.errorMessage = this.resolveError(error);
        this.loading = false;
      }
    });
  }

  googleLogin(): void {
    this.loading = true;
    this.authService.getGoogleLoginUrl().subscribe({
      next: (url) => {
        window.location.href = url.replace(/^"|"$/g, '');
      },
      error: (error) => {
        this.errorMessage = this.resolveError(error);
        this.loading = false;
      }
    });
  }

  private resolveError(error: unknown): string {
    const response = error as { error?: { title?: string; detail?: string }; message?: string };
    return response?.error?.detail ?? response?.error?.title ?? response?.message ?? 'Unable to sign in.';
  }
}
