import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly baseURL = 'https://localhost:7285/api/Auth';
  private readonly tokenKey = 'auth_token';
  private readonly http = inject(HttpClient);

  login(credentials: any): Observable<any> {
    return this.http.post<any>(`${this.baseURL}/login`, credentials).pipe(
      tap((response) => {
        if (response && response.token) {
          this.saveToken(response.token);
        }
      })
    );
  }

  register(userData: any): Observable<any> {
    return this.http.post<any>(`${this.baseURL}/register`, userData);
  }

  saveToken(token: string): void {
    localStorage.setItem(this.tokenKey, token);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  removeToken(): void {
    localStorage.removeItem(this.tokenKey);
  }

  logout(): void {
    this.removeToken();
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }
}
