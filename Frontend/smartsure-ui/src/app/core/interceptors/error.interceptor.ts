import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, Observable, throwError } from 'rxjs';
import { AuthStateService } from '../services/auth-state.service';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor(private readonly authState: AuthStateService, private readonly router: Router) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      catchError((error: unknown) => {
        if (error instanceof HttpErrorResponse && error.status === 401) {
          this.authState.clearSession();
          void this.router.navigateByUrl('/auth/login');
        }

        return throwError(() => error);
      })
    );
  }
}
