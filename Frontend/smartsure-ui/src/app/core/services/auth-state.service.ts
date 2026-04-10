import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { AuthResponseDto } from '../../models/api-models';

export interface AuthSession extends AuthResponseDto {
  expiresAtUtc: string;
}

@Injectable({ providedIn: 'root' })
export class AuthStateService {
  private readonly storageKey = 'smartsure.auth.session';
  private readonly sessionSubject = new BehaviorSubject<AuthSession | null>(null);
  readonly session$ = this.sessionSubject.asObservable();

  restoreSession(): void {
    if (this.sessionSubject.value) {
      return;
    }

    const rawSession = localStorage.getItem(this.storageKey);
    if (!rawSession) {
      return;
    }

    try {
      const session = JSON.parse(rawSession) as AuthSession;
      this.sessionSubject.next(session);
    } catch {
      localStorage.removeItem(this.storageKey);
    }
  }

  setSession(session: AuthResponseDto): void {
    const normalizedSession: AuthSession = {
      ...session,
      roles: session.roles ?? []
    };

    localStorage.setItem(this.storageKey, JSON.stringify(normalizedSession));
    this.sessionSubject.next(normalizedSession);
  }

  clearSession(): void {
    localStorage.removeItem(this.storageKey);
    this.sessionSubject.next(null);
  }

  get snapshot(): AuthSession | null {
    return this.sessionSubject.value;
  }

  get token(): string | null {
    return this.sessionSubject.value?.token ?? null;
  }

  get refreshToken(): string | null {
    return this.sessionSubject.value?.refreshToken ?? null;
  }

  get userName(): string {
    return this.sessionSubject.value?.fullName ?? 'Guest';
  }

  get roles(): string[] {
    return this.sessionSubject.value?.roles ?? [];
  }

  get isAuthenticated(): boolean {
    return Boolean(this.token);
  }

  hasRole(role: string): boolean {
    return this.roles.includes(role);
  }

  hasAnyRole(roles: string[]): boolean {
    return roles.some((role) => this.hasRole(role));
  }

  primaryRoute(): string {
    return this.hasRole('ADMIN') ? '/admin/dashboard' : '/customer/dashboard';
  }

  updateFullName(name: string): void {
    const current = this.sessionSubject.value;
    if (!current) return;
    const updated = { ...current, fullName: name };
    localStorage.setItem(this.storageKey, JSON.stringify(updated));
    this.sessionSubject.next(updated);
  }
}
