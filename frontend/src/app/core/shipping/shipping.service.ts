import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Governorate } from './shipping.models';

@Injectable({
  providedIn: 'root',
})
export class ShippingService {
  private readonly http = inject(HttpClient);
  private readonly optionsUrl = `${environment.apiUrl}/shipping/options`;

  getOptions(): Observable<Governorate[]> {
    return this.http.get<Governorate[]>(this.optionsUrl);
  }
}
