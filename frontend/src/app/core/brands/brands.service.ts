import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { BrandsResponse } from './brands.models';

@Injectable({
  providedIn: 'root',
})
export class BrandsService {
  private readonly http = inject(HttpClient);
  private readonly brandsUrl = `${environment.apiUrl}/brands`;

  getBrands(): Observable<BrandsResponse> {
    return this.http.get<BrandsResponse>(this.brandsUrl);
  }
}
