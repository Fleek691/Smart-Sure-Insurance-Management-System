import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ApiService {
  constructor(private readonly http: HttpClient) {}

  get<T>(path: string, params?: Record<string, string | number | boolean | undefined | null>) {
    return this.http.get<T>(this.url(path), { params: this.buildParams(params) });
  }

  post<T>(path: string, body?: unknown, params?: Record<string, string | number | boolean | undefined | null>) {
    return this.http.post<T>(this.url(path), body ?? {}, { params: this.buildParams(params) });
  }

  put<T>(path: string, body?: unknown, params?: Record<string, string | number | boolean | undefined | null>) {
    return this.http.put<T>(this.url(path), body ?? {}, { params: this.buildParams(params) });
  }

  delete<T>(path: string, params?: Record<string, string | number | boolean | undefined | null>) {
    return this.http.delete<T>(this.url(path), { params: this.buildParams(params) });
  }

  download(path: string, params?: Record<string, string | number | boolean | undefined | null>) {
    return this.http.get(this.url(path), { params: this.buildParams(params), responseType: 'blob' });
  }

  text(path: string, params?: Record<string, string | number | boolean | undefined | null>) {
    return this.http.get(this.url(path), { params: this.buildParams(params), responseType: 'text' });
  }

  private url(path: string): string {
    return `${environment.apiBaseUrl}${path.startsWith('/') ? path : `/${path}`}`;
  }

  private buildParams(params?: Record<string, string | number | boolean | undefined | null>): HttpParams | undefined {
    if (!params) {
      return undefined;
    }

    let httpParams = new HttpParams();
    for (const [key, value] of Object.entries(params)) {
      if (value !== undefined && value !== null && value !== '') {
        httpParams = httpParams.set(key, String(value));
      }
    }

    return httpParams;
  }
}
