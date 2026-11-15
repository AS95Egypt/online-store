import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AdminProductDetails,
  AdminProductListItem,
  AdminProductRequest,
  AdminProductsResponse,
} from './admin-products.models';

@Injectable({
  providedIn: 'root',
})
export class AdminProductsService {
  private readonly http = inject(HttpClient);
  private readonly productsUrl = `${environment.apiUrl}/admin/products`;

  list(page: number, pageSize: number, search: string): Observable<AdminProductsResponse<AdminProductListItem[]>> {
    const params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize)
      .set('search', search.trim());
    return this.http.get<AdminProductsResponse<AdminProductListItem[]>>(this.productsUrl, { params });
  }

  getById(id: number): Observable<AdminProductsResponse<AdminProductDetails>> {
    return this.http.get<AdminProductsResponse<AdminProductDetails>>(`${this.productsUrl}/${id}`);
  }

  create(request: AdminProductRequest): Observable<AdminProductsResponse<AdminProductDetails>> {
    return this.http.post<AdminProductsResponse<AdminProductDetails>>(this.productsUrl, request);
  }

  update(id: number, request: AdminProductRequest): Observable<AdminProductsResponse<AdminProductDetails>> {
    return this.http.put<AdminProductsResponse<AdminProductDetails>>(`${this.productsUrl}/${id}`, request);
  }

  setStatus(id: number, isActive: boolean): Observable<AdminProductsResponse<AdminProductDetails>> {
    return this.http.patch<AdminProductsResponse<AdminProductDetails>>(
      `${this.productsUrl}/${id}/status`,
      { isActive },
    );
  }
}
