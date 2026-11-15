import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from '../auth/auth.service';
import {
  ChangePasswordRequest,
  CustomerAddress,
  CustomerProfile,
  UpdateCustomerAddressRequest,
  UpdateCustomerProfileRequest,
} from './account.models';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  private readonly http = inject(HttpClient);
  private readonly authService = inject(AuthService);
  private readonly baseUrl = `${environment.apiUrl}/customers/me`;
  private readonly addressUrl = `${environment.apiUrl}/customers/address`;

  getProfile(): Observable<CustomerProfile> {
    return this.http.get<CustomerProfile>(this.baseUrl).pipe(
      tap((profile) => this.authService.setCurrentUser(profile)),
    );
  }

  updateProfile(request: UpdateCustomerProfileRequest): Observable<CustomerProfile> {
    return this.http.put<CustomerProfile>(this.baseUrl, request).pipe(
      tap((profile) => this.authService.setCurrentUser(profile)),
    );
  }

  changePassword(request: ChangePasswordRequest): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/change-password`, request);
  }

  getAddress(): Observable<CustomerAddress> {
    return this.http.get<CustomerAddress>(this.addressUrl);
  }

  updateAddress(request: UpdateCustomerAddressRequest): Observable<CustomerAddress> {
    return this.http.put<CustomerAddress>(this.addressUrl, request);
  }
}
