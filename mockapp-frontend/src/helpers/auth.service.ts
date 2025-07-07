import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { jwtDecode } from 'jwt-decode';
import { CreateUserInput } from './auth.model';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = `https://localhost:44313/api/auth`;

  constructor(private _http: HttpClient, private _router: Router) {}

  register(input: CreateUserInput): Observable<{ token: string }> {
    return this._http
      .post<{ token: string }>(`${this.apiUrl}/register`, input)
      .pipe(
        tap((res) => {
          localStorage.setItem('token', res.token);
          this._router.navigate(['/mocks']);
        })
      );
  }

  login(email: string, password: string): Observable<{ token: string }> {
    return this._http
      .post<{ token: string }>(`${this.apiUrl}/login`, {
        email,
        password,
      })
      .pipe(
        tap((res) => {
          localStorage.setItem('token', res.token);
          this._router.navigate(['/mocks']);
        })
      );
  }

  logout() {
    localStorage.removeItem('token');
    this._router.navigate(['/login']);
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem('token');
  }

  getUserId(): string | null {
    const token = localStorage.getItem('token');
    if (!token) return null;
    const decoded = jwtDecode(token);
    return decoded?.sub || null;
  }
}
