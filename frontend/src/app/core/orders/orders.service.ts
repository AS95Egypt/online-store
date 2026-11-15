import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CheckoutCalculationResult } from '../cart/cart.models';
import { CreateOrderRequest, OrderDetails, OrderSummary } from './orders.models';

@Injectable({
  providedIn: 'root',
})
export class OrdersService {
  private readonly http = inject(HttpClient);
  private readonly ordersUrl = `${environment.apiUrl}/orders`;

  createOrder(
    clientRequestId: string,
    checkout: CheckoutCalculationResult,
  ): Observable<OrderDetails> {
    const request: CreateOrderRequest = {
      clientRequestId,
      expectedItems: (checkout.items ?? []).map((item) => ({
        productId: item.productId,
        quantity: item.quantity,
        unitPrice: item.unitPrice,
      })),
      expectedShippingFee: checkout.shippingFee ?? 0,
    };
    return this.http.post<OrderDetails>(this.ordersUrl, request);
  }

  getOrders(): Observable<OrderSummary[]> {
    return this.http.get<OrderSummary[]>(this.ordersUrl);
  }

  getOrderDetails(orderNumber: string): Observable<OrderDetails> {
    return this.http.get<OrderDetails>(`${this.ordersUrl}/${encodeURIComponent(orderNumber)}`);
  }

  cancelOrder(orderNumber: string): Observable<OrderDetails> {
    return this.http.post<OrderDetails>(
      `${this.ordersUrl}/${encodeURIComponent(orderNumber)}/cancel`,
      {},
    );
  }
}
