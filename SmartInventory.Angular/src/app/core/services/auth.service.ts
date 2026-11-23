import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { Router } from '@angular/router';

import { environment } from '../../../environments/environment';
import { LoginRequest } from '../models/auth/login-request';
import {
  ApiResponse,
  AuthResponseData
} from '../models/auth/auth-response';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private http = inject(HttpClient);
  private router = inject(Router);

  private readonly apiUrl = environment.apiBaseUrl;

  login(request: LoginRequest): Observable<ApiResponse<AuthResponseData>> {

    return this.http.post<ApiResponse<AuthResponseData>>(
      `${this.apiUrl}api/Auth/login`,
      request
    ).pipe(
      tap(response => {
        if (response.data?.token) {
          sessionStorage.setItem(
            'accessToken',
            response.data.token
          );

          sessionStorage.setItem(
            'userData',
            JSON.stringify({
              userId: response.data.userId,
              fullName: response.data.fullName,
              email: response.data.email,
              roles: response.data.roles,
              expiresAt: response.data.expiresAt
            })
          );
        }
      })
    );
  }

  getToken(): string | null {
    const token = sessionStorage.getItem('accessToken');

    if (!token || !this.isAuthenticated()) {
      this.clearSession();
      return null;
    }

    return token;
  }

  getUser(): AuthResponseData | null {
    const userData = sessionStorage.getItem('userData');

    if (!userData || !this.isAuthenticated()) {
      return null;
    }

    return JSON.parse(userData) as AuthResponseData;
  }

  isAuthenticated(): boolean {
    const token = sessionStorage.getItem('accessToken');
    const userData = sessionStorage.getItem('userData');

    if (!token || !userData) {
      return false;
    }

    try {
      const user = JSON.parse(userData) as AuthResponseData;

      return new Date(user.expiresAt).getTime() > Date.now();
    } catch {
      return false;
    }
  }

  hasRole(roles: string[]): boolean {
    const user = this.getUser();

    if (!user) {
      return false;
    }

    return roles.some(role => user.roles.includes(role));
  }

  logout(): void {
    this.clearSession();
    this.router.navigate(['/login']);
  }

  private clearSession(): void {
    sessionStorage.removeItem('accessToken');
    sessionStorage.removeItem('userData');
  }
}