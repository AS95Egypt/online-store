import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AdminApiResponse,
  AdminOrderDetails,
  AdminOrderListResult,
  AdminOrderStatus,
} from './admin-orders.models';

@Injectable({
  providedIn: 'root',
})
export class AdminOrdersService {
  private readonly http = inject(HttpClient);
  private readonly ordersUrl = `${environment.apiUrl}/admin/orders`;

  getOrders(
    status: string,
    search: string,
    page: number,
    pageSize: number,
  ): Observable<AdminOrderListResult> {
    const params = new HttpParams()
      .set('status', status)
      .set('search', search.trim())
      .set('page', page)
      .set('pageSize', pageSize);
    return this.http
      .get<AdminApiResponse<AdminOrderListResult>>(this.ordersUrl, { params })
      .pipe(map((response) => response.data));
  }

  getOrderDetails(id: number): Observable<AdminOrderDetails> {
    return this.http
      .get<AdminApiResponse<AdminOrderDetails>>(`${this.ordersUrl}/${id}`)
      .pipe(map((response) => response.data));
  }

  updateStatus(id: number, status: AdminOrderStatus): Observable<AdminOrderDetails> {
    return this.http
      .patch<AdminApiResponse<AdminOrderDetails>>(`${this.ordersUrl}/${id}/status`, { status })
      .pipe(map((response) => response.data));
  }

  cancelOrder(id: number): Observable<AdminOrderDetails> {
    return this.http
      .post<AdminApiResponse<AdminOrderDetails>>(`${this.ordersUrl}/${id}/cancel`, {})
      .pipe(map((response) => response.data));
  }

  markPaid(id: number): Observable<AdminOrderDetails> {
    return this.http
      .post<AdminApiResponse<AdminOrderDetails>>(`${this.ordersUrl}/${id}/payment/paid`, {})
      .pipe(map((response) => response.data));
  }
}
