import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, map, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CurrentUser } from '../account/account.models';
import {
  AuthResponse,
  ForgotPasswordRequest,
  LoginRequest,
  RegisterRequest,
  ResetPasswordRequest,
  UserRole,
  VerifyOtpRequest,
} from './auth.models';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly tokenKey = 'online_store_token';
  private readonly expiryKey = 'online_store_token_expiry';
  private readonly roleKey = 'online_store_user_role';
  private readonly authStateSubject = new BehaviorSubject<boolean>(this.isAuthenticated());
  private readonly currentUserSubject = new BehaviorSubject<CurrentUser | null>(null);

  readonly isAuthenticated$ = this.authStateSubject.asObservable();
  readonly currentUser$ = this.currentUserSubject.asObservable();
  readonly currentRole$ = this.currentUserSubject
    .asObservable()
    .pipe(map((user) => user?.role ?? this.getCurrentRole()));

  register(request: RegisterRequest): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/auth/register`, request);
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${environment.apiUrl}/auth/login`, request)
      .pipe(tap((response) => this.persistSession(response)));
  }

  logout(): void {
    this.clearSession();
  }

  forgotPassword(request: ForgotPasswordRequest): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/auth/forgot-password`, request);
  }

  verifyOtp(request: VerifyOtpRequest): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/auth/verify-otp`, request);
  }

  resetPassword(request: ResetPasswordRequest): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/auth/reset-password`, request);
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    const expiry = Number(localStorage.getItem(this.expiryKey) ?? '0');

    return Boolean(token) && expiry > Date.now();
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  clearSession(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.expiryKey);
    localStorage.removeItem(this.roleKey);
    this.authStateSubject.next(false);
    this.currentUserSubject.next(null);
  }

  setCurrentUser(user: CurrentUser | null): void {
    this.currentUserSubject.next(user);
    if (user) {
      localStorage.setItem(this.roleKey, user.role);
    } else {
      localStorage.removeItem(this.roleKey);
    }
  }

  getCurrentRole(): UserRole | null {
    const role = this.currentUserSubject.value?.role ?? localStorage.getItem(this.roleKey);
    return role === 'ADMIN' || role === 'CUSTOMER' ? role : null;
  }

  private persistSession(response: AuthResponse): void {
    const expiresAt = new Date(response.expiresAtUtc).getTime();

    localStorage.setItem(this.tokenKey, response.token);
    localStorage.setItem(this.expiryKey, expiresAt.toString());
    this.setCurrentUser({
      name: response.fullName,
      email: response.email,
      role: response.role,
    });
    this.authStateSubject.next(true);
  }
}
