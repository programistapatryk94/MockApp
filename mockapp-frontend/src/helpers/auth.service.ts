import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { jwtDecode, JwtPayload } from 'jwt-decode';
import { AuthResponse, CreateUserInput } from './auth.model';
import { AppConfigService } from '../app/shared/app-config.service';
import { AppConsts } from '../app/shared/AppConsts';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private tokenSubject = new BehaviorSubject<string | null>(null);
  public token$ = this.tokenSubject.asObservable();

  get apiUrl(): string {
    return `${this.config.remoteApiUrl}/api/auth`;
  }

  constructor(
    private http: HttpClient,
    private router: Router,
    private config: AppConfigService
  ) {
    const token = localStorage.getItem('token');
    this.tokenSubject.next(token);
  }

  register(input: CreateUserInput): Observable<AuthResponse> {
    return this.http
      .post<{ token: string }>(`${this.apiUrl}/register`, input)
      .pipe(tap((res) => this.handleAuthSuccess(res.token)));
  }

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http
      .post<{ token: string }>(`${this.apiUrl}/login`, {
        email,
        password,
      })
      .pipe(tap((res) => this.handleAuthSuccess(res.token)));
  }

  getToken(): string | null {
    return this.tokenSubject.value;
  }

  logout() {
    localStorage.removeItem('token');
    this.tokenSubject.next(null);
    location.href = AppConsts.appBaseUrl;
  }

  isLoggedIn(): boolean {
    const token = this.getToken();
    return !!token && !this.isTokenExpired();
  }

  isTokenExpired(): boolean {
    const token = this.getToken();
    if (!token) return true;

    const decoded = jwtDecode<{ exp: number }>(token);
    const now = Date.now() / 1000;

    return decoded.exp < now;
  }

  getUserId(): string | null {
    const token = this.getToken();
    if (!token) return null;

    try {
      const decoded = jwtDecode<JwtPayload>(token);
      return decoded?.sub || null;
    } catch (error) {
      console.warn('Nieprawidłowy token:', error);
      return null;
    }
  }

  private handleAuthSuccess(token: string): void {
    localStorage.setItem('token', token);
    this.tokenSubject.next(token);
    
    location.href = AppConsts.appBaseUrl;
  }
}
