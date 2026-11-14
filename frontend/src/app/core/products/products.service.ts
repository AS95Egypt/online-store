import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult, ProductDetails, ProductListFilters, ProductListItem } from './products.models';

@Injectable({
  providedIn: 'root',
})
export class ProductsService {
  private readonly http = inject(HttpClient);
  private readonly productsUrl = `${environment.apiUrl}/products`;

  getProducts(
    page: number,
    pageSize: number,
    filters: ProductListFilters = {},
  ): Observable<PagedResult<ProductListItem>> {
    const params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);

    const filterParams: [string, string | undefined][] = [
      ['search', filters.search],
      ['categoryId', filters.categoryId],
      ['brandIds', filters.brandIds],
      ['minPrice', filters.minPrice],
      ['maxPrice', filters.maxPrice],
      ['stockStatus', filters.stockStatus],
      ['sort', filters.sort],
    ];
    const filteredParams = filterParams.reduce(
      (current, [key, value]) =>
        value?.trim() ? current.set(key, value) : current,
      params,
    );

    return this.http.get<PagedResult<ProductListItem>>(this.productsUrl, { params: filteredParams });
  }

  getProductById(id: number): Observable<ProductDetails> {
    return this.http.get<ProductDetails>(`${this.productsUrl}/${id}`);
  }

  // Cloudinary unsigned upload and 1 MB image validation belong to the admin product-management story (F2).
}
