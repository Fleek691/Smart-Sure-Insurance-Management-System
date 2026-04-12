import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, Observable, throwError } from 'rxjs';
import { AuthStateService } from '../services/auth-state.service';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor(
    private readonly authState: AuthStateService,
    private readonly router: Router
  ) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      catchError((error: unknown) => {
        if (!(error instanceof HttpErrorResponse)) {
          return throwError(() => error);
        }

        switch (error.status) {
          case 0:
            // Network error — server unreachable or CORS preflight failed
            return throwError(() => this.synthetic(error, 'Unable to reach the server. Please check your connection and try again.'));

          case 401:
            // Only clear session on 401 from authenticated routes
            // (auth endpoints like /login return 401 for wrong credentials — don't redirect those)
            if (this.authState.isAuthenticated) {
              this.authState.clearSession();
              void this.router.navigateByUrl('/auth/login');
            }
            return throwError(() => error);

          case 403:
            return throwError(() => this.synthetic(error, 'You do not have permission to perform this action.'));

          case 404:
            return throwError(() => error); // let the component handle with its own message

          case 409:
            return throwError(() => error); // conflict — component shows the detail

          case 422:
            return throwError(() => error); // business rule — component shows the detail

          case 429:
            return throwError(() => this.synthetic(error, 'Too many requests. Please wait a moment and try again.'));

          case 500:
            return throwError(() => this.synthetic(error, 'An unexpected server error occurred. Please try again later.'));

          case 502:
          case 503:
          case 504:
            return throwError(() => this.synthetic(error, 'The service is temporarily unavailable. Please try again in a few moments.'));

          default:
            return throwError(() => error);
        }
      })
    );
  }

  /** Wraps a synthetic detail message into the same problem+json shape the backend uses. */
  private synthetic(original: HttpErrorResponse, detail: string): HttpErrorResponse {
    return new HttpErrorResponse({
      error: { detail, title: detail, status: original.status },
      status: original.status,
      statusText: original.statusText,
      url: original.url ?? undefined
    });
  }
}
