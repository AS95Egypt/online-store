import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const token = authService.getToken();

  const shouldSkipAuth =
    req.url.includes('/auth/login') ||
    req.url.includes('/auth/register') ||
    req.url.includes('/auth/forgot-password') ||
    req.url.includes('/auth/verify-otp') ||
    req.url.includes('/auth/reset-password');

  const authRequest = token && !shouldSkipAuth ? req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    }) : req;

  return next(authRequest).pipe(
    catchError((error) => {
      if (error.status === 401) {
        authService.clearSession();
        router.navigateByUrl('/login');
      }

      return throwError(() => error);
    }),
  );
};
