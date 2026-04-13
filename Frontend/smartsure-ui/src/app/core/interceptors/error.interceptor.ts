import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, catchError, filter, Observable, switchMap, take, throwError } from 'rxjs';
import { AuthStateService } from '../services/auth-state.service';
import { AuthService } from '../services/auth.service';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  private refreshing = false;
  private refreshDone$ = new BehaviorSubject<boolean>(false);

  constructor(
    private readonly authState: AuthStateService,
    private readonly authService: AuthService,
    private readonly router: Router
  ) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      catchError((error: unknown) => {
        if (!(error instanceof HttpErrorResponse)) {
          return throwError(() => error);
        }

        // ── 401 — try token refresh first, then logout ──────────────────
        if (error.status === 401 && this.authState.isAuthenticated) {
          // Don't retry the refresh endpoint itself
          if (request.url.includes('/auth/refresh-token')) {
            this.logout();
            return throwError(() => error);
          }

          if (this.refreshing) {
            // Queue this request until refresh completes
            return this.refreshDone$.pipe(
              filter(done => done),
              take(1),
              switchMap(() => next.handle(this.addToken(request)))
            );
          }

          return this.doRefresh(request, next, error);
        }

        // ── Other status codes ──────────────────────────────────────────
        return throwError(() => this.enrich(error));
      })
    );
  }

  private doRefresh(
    request: HttpRequest<unknown>,
    next: HttpHandler,
    originalError: HttpErrorResponse
  ): Observable<HttpEvent<unknown>> {
    this.refreshing = true;
    this.refreshDone$.next(false);

    const refreshToken = this.authState.refreshToken;
    if (!refreshToken) {
      this.logout();
      return throwError(() => originalError);
    }

    return this.authService.refreshToken(refreshToken).pipe(
      switchMap(session => {
        this.authState.setSession(session);
        this.refreshing = false;
        this.refreshDone$.next(true);
        return next.handle(this.addToken(request));
      }),
      catchError(err => {
        this.refreshing = false;
        this.refreshDone$.next(true);
        this.logout();
        return throwError(() => err);
      })
    );
  }

  private addToken(request: HttpRequest<unknown>): HttpRequest<unknown> {
    const token = this.authState.token;
    return token
      ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : request;
  }

  private logout(): void {
    this.authState.clearSession();
    void this.router.navigateByUrl('/auth/login');
  }

  /** Attach a human-readable detail to well-known HTTP errors. */
  private enrich(error: HttpErrorResponse): HttpErrorResponse {
    const knownMessages: Record<number, string> = {
      0:   'Unable to reach the server. Please check your connection.',
      403: 'You do not have permission to perform this action.',
      429: 'Too many requests. Please wait a moment and try again.',
      500: 'An unexpected server error occurred. Please try again later.',
      502: 'The service is temporarily unavailable. Please try again in a few moments.',
      503: 'The service is temporarily unavailable. Please try again in a few moments.',
      504: 'The service is temporarily unavailable. Please try again in a few moments.',
    };

    const msg = knownMessages[error.status];
    if (!msg) return error;

    return new HttpErrorResponse({
      error: { detail: msg, title: msg, status: error.status },
      status: error.status,
      statusText: error.statusText,
      url: error.url ?? undefined
    });
  }
}
