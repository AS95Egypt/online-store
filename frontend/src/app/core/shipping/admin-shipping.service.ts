import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AdminGovernorate,
  AdminShippingApiResponse,
  UpdateGovernorateFeeRequest,
} from './admin-shipping.models';

@Injectable({
  providedIn: 'root',
})
export class AdminShippingService {
  private readonly http = inject(HttpClient);
  private readonly governoratesUrl = `${environment.apiUrl}/admin/shipping/governorates`;

  getGovernorates(): Observable<AdminGovernorate[]> {
    return this.http
      .get<AdminShippingApiResponse<AdminGovernorate[]>>(this.governoratesUrl)
      .pipe(map((response) => response.data));
  }

  updateFee(id: number, request: UpdateGovernorateFeeRequest): Observable<AdminGovernorate> {
    return this.http
      .put<AdminShippingApiResponse<AdminGovernorate>>(`${this.governoratesUrl}/${id}`, request)
      .pipe(map((response) => response.data));
  }
}
