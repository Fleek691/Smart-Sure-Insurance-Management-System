import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthStateService } from '../../core/services/auth-state.service';
import { AuthResponseDto } from '../../models/api-models';

@Component({
  selector: 'app-google-callback',
  standalone: true,
  template: `
    <div class="callback-shell">
      <div class="spinner-box">
        <svg class="spin" viewBox="0 0 50 50">
          <circle cx="25" cy="25" r="20" fill="none" stroke="#5465FF" stroke-width="4"
                  stroke-dasharray="100" stroke-dashoffset="60" stroke-linecap="round"/>
        </svg>
        <p>{{ message }}</p>
      </div>
    </div>
  `,
  styles: [`
    .callback-shell {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: #fff;
    }
    .spinner-box {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 1.2rem;
    }
    .spin {
      width: 48px;
      height: 48px;
      animation: rotate 0.9s linear infinite;
    }
    @keyframes rotate { to { transform: rotate(360deg); } }
    p {
      color: #5f6b82;
      font-size: 0.97rem;
      margin: 0;
    }
  `]
})
export class GoogleCallbackComponent implements OnInit {
  message = 'Signing you in…';

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly authState: AuthStateService
  ) {}

  ngOnInit(): void {
    const p = this.route.snapshot.queryParams as Record<string, string>;

    const token      = p['token'];
    const userId     = p['userId'];
    const fullName   = p['fullName'];
    const email      = p['email'];
    const refreshToken = p['refreshToken'];
    const expiresAtUtc = p['expiresAtUtc'];
    const rolesRaw   = p['roles'];

    if (!token || !userId) {
      this.message = 'Sign-in failed. Redirecting…';
      setTimeout(() => void this.router.navigateByUrl('/auth/login'), 2000);
      return;
    }

    const session: AuthResponseDto = {
      userId,
      fullName: fullName ?? '',
      email: email ?? '',
      token,
      refreshToken: refreshToken ?? '',
      expiresAtUtc: expiresAtUtc ?? new Date().toISOString(),
      roles: rolesRaw ? rolesRaw.split(',').filter(Boolean) : []
    };

    this.authState.setSession(session);
    void this.router.navigateByUrl(this.authState.primaryRoute());
  }
}
