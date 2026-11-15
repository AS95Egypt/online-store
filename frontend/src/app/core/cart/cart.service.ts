import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AddCartItemRequest,
  Cart,
  CheckoutCalculationResult,
  UpdateCartItemRequest,
} from './cart.models';

@Injectable({
  providedIn: 'root',
})
export class CartService {
  private readonly http = inject(HttpClient);
  private readonly cartUrl = `${environment.apiUrl}/cart`;

  readonly itemCount = signal(0);

  getCart(): Observable<Cart> {
    return this.http.get<Cart>(this.cartUrl).pipe(tap((cart) => this.updateItemCount(cart)));
  }

  addItem(productId: number, quantity = 1): Observable<Cart> {
    const request: AddCartItemRequest = { productId, quantity };
    return this.http.post<Cart>(`${this.cartUrl}/items`, request).pipe(
      tap((cart) => this.updateItemCount(cart)),
    );
  }

  updateItem(productId: number, quantity: number): Observable<Cart> {
    const request: UpdateCartItemRequest = { quantity };
    return this.http.put<Cart>(`${this.cartUrl}/items/${productId}`, request).pipe(
      tap((cart) => this.updateItemCount(cart)),
    );
  }

  removeItem(productId: number): Observable<Cart> {
    return this.http.delete<Cart>(`${this.cartUrl}/items/${productId}`).pipe(
      tap((cart) => this.updateItemCount(cart)),
    );
  }

  checkout(governorateId?: number): Observable<CheckoutCalculationResult> {
    const url = governorateId === undefined
      ? `${this.cartUrl}/checkout`
      : `${this.cartUrl}/checkout?governorateId=${governorateId}`;
    return this.http.post<CheckoutCalculationResult>(url, {});
  }

  clearItemCount(): void {
    this.itemCount.set(0);
  }

  private updateItemCount(cart: Cart): void {
    this.itemCount.set(cart.items.reduce((total, item) => total + item.quantity, 0));
  }
}
