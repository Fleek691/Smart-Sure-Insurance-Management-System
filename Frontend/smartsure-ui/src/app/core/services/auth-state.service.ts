import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { AuthResponseDto } from '../../models/api-models';

@Injectable({ providedIn: 'root' })
export class AuthStateService {
  private readonly storageKey = 'smartsure.auth.session';
  private readonly sessionSubject = new BehaviorSubject<AuthResponseDto | null>(null);
  readonly session$ = this.sessionSubject.asObservable();

  restoreSession(): void {
    if (this.sessionSubject.value) return;

    const raw = localStorage.getItem(this.storageKey);
    if (!raw) return;

    try {
      this.sessionSubject.next(JSON.parse(raw) as AuthResponseDto);
    } catch {
      localStorage.removeItem(this.storageKey);
    }
  }

  setSession(session: AuthResponseDto): void {
    const normalized: AuthResponseDto = { ...session, roles: session.roles ?? [] };
    localStorage.setItem(this.storageKey, JSON.stringify(normalized));
    this.sessionSubject.next(normalized);
  }

  clearSession(): void {
    localStorage.removeItem(this.storageKey);
    this.sessionSubject.next(null);
  }

  get snapshot(): AuthResponseDto | null {
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
    return roles.some((r) => this.hasRole(r));
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
