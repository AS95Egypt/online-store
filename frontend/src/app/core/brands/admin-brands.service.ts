import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AdminBrand, AdminBrandRequest, AdminBrandsResponse } from './admin-brands.models';

@Injectable({
  providedIn: 'root',
})
export class AdminBrandsService {
  private readonly http = inject(HttpClient);
  private readonly brandsUrl = `${environment.apiUrl}/admin/brands`;

  list(): Observable<AdminBrandsResponse<AdminBrand[]>> {
    return this.http.get<AdminBrandsResponse<AdminBrand[]>>(this.brandsUrl);
  }

  create(request: AdminBrandRequest): Observable<AdminBrandsResponse<AdminBrand>> {
    return this.http.post<AdminBrandsResponse<AdminBrand>>(this.brandsUrl, request);
  }

  update(id: number, request: AdminBrandRequest): Observable<AdminBrandsResponse<AdminBrand>> {
    return this.http.put<AdminBrandsResponse<AdminBrand>>(`${this.brandsUrl}/${id}`, request);
  }
}
