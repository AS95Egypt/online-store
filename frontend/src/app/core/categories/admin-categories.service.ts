import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AdminCategoriesResponse,
  AdminCategory,
  AdminCategoryRequest,
} from './admin-categories.models';

@Injectable({
  providedIn: 'root',
})
export class AdminCategoriesService {
  private readonly http = inject(HttpClient);
  private readonly categoriesUrl = `${environment.apiUrl}/admin/categories`;

  list(): Observable<AdminCategoriesResponse<AdminCategory[]>> {
    return this.http.get<AdminCategoriesResponse<AdminCategory[]>>(this.categoriesUrl);
  }

  create(request: AdminCategoryRequest): Observable<AdminCategoriesResponse<AdminCategory>> {
    return this.http.post<AdminCategoriesResponse<AdminCategory>>(this.categoriesUrl, request);
  }

  update(id: number, request: AdminCategoryRequest): Observable<AdminCategoriesResponse<AdminCategory>> {
    return this.http.put<AdminCategoriesResponse<AdminCategory>>(`${this.categoriesUrl}/${id}`, request);
  }

  setStatus(id: number, isActive: boolean): Observable<AdminCategoriesResponse<AdminCategory>> {
    return this.http.patch<AdminCategoriesResponse<AdminCategory>>(
      `${this.categoriesUrl}/${id}/status`,
      { isActive },
    );
  }
}
