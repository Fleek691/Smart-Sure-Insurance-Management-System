import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthStateService } from '../services/auth-state.service';
import { environment } from '../../../environments/environment';

@Injectable()
export class JwtInterceptor implements HttpInterceptor {
  constructor(private readonly authState: AuthStateService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const isGatewayRequest = request.url.startsWith(environment.apiBaseUrl);
    const token = this.authState.token;
    const headers: Record<string, string> = {};

    if (isGatewayRequest && !request.headers.has('X-Client-Id')) {
      headers['X-Client-Id'] = 'smartsure-ui-web';
    }

    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    if (Object.keys(headers).length === 0) {
      return next.handle(request);
    }

    return next.handle(request.clone({ setHeaders: headers }));
  }
}
