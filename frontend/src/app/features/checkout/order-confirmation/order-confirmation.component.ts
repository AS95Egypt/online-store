import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { OrdersService } from '../../../core/orders/orders.service';
import { OrderDetails } from '../../../core/orders/orders.models';

@Component({
  selector: 'app-order-confirmation',
  standalone: true,
  imports: [DecimalPipe, RouterLink],
  template: `
    <section class="confirmation-page">
      @if (isLoading()) {
        <p role="status">Loading your order…</p>
      } @else if (error()) {
        <div class="confirmation-card">
          <h1>Order confirmation</h1>
          <p class="error" role="alert">{{ error() }}</p>
          <a routerLink="/orders">Go to My Orders</a>
        </div>
      } @else if (order(); as currentOrder) {
        <div class="confirmation-card">
          <div class="success-mark" aria-hidden="true">✓</div>
          <p class="eyebrow">Order received</p>
          <h1>Thank you for your order</h1>
          <p class="description">Your order number is</p>
          <strong class="order-number">{{ currentOrder.orderNumber }}</strong>
          <p class="total">Total: {{ currentOrder.total | number: '1.0-2' }} EGP</p>
          <p class="cod-note">Payment is Cash on Delivery.</p>
          <div class="actions">
            <a routerLink="/">Continue shopping</a>
            <a routerLink="/orders" class="secondary-link">View My Orders</a>
          </div>
        </div>
      }
    </section>
  `,
  styles: `
    .confirmation-page {
      display: grid;
      min-height: 65vh;
      place-items: center;
      padding: 2rem 1rem;
    }

    .confirmation-card {
      display: grid;
      justify-items: center;
      gap: 0.75rem;
      width: min(100%, 520px);
      padding: clamp(1.5rem, 5vw, 3rem);
      border: 1px solid var(--line);
      border-radius: 16px;
      background: var(--panel);
      text-align: center;
    }

    .success-mark {
      display: grid;
      width: 3.5rem;
      height: 3.5rem;
      place-items: center;
      border-radius: 50%;
      background: #e8f5ee;
      color: #1f7a53;
      font-size: 1.8rem;
      font-weight: 700;
    }

    .eyebrow,
    h1,
    p {
      margin: 0;
    }

    .eyebrow {
      color: var(--primary);
      font-size: 0.72rem;
      font-weight: 700;
      letter-spacing: 0.12em;
      text-transform: uppercase;
    }

    h1 {
      color: var(--text);
      font-size: clamp(1.6rem, 4vw, 2rem);
    }

    .description,
    .cod-note {
      color: var(--muted);
    }

    .order-number {
      color: var(--text);
      font-size: 1.15rem;
      letter-spacing: 0.04em;
    }

    .total {
      color: var(--primary);
      font-size: 1.1rem;
      font-weight: 700;
    }

    .actions {
      display: flex;
      flex-wrap: wrap;
      justify-content: center;
      gap: 0.75rem;
      margin-top: 0.75rem;
    }

    a {
      color: var(--primary);
      font-weight: 600;
      text-decoration: none;
    }

    .secondary-link {
      padding-left: 0.75rem;
      border-left: 1px solid var(--line);
    }

    .error {
      color: var(--danger);
    }
  `,
})
export class OrderConfirmationComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly ordersService = inject(OrdersService);

  readonly order = signal<OrderDetails | null>(null);
  readonly isLoading = signal(true);
  readonly error = signal('');

  ngOnInit(): void {
    const orderNumber = this.route.snapshot.paramMap.get('orderNumber');
    if (!orderNumber) {
      this.error.set('The order number is missing.');
      this.isLoading.set(false);
      return;
    }

    this.ordersService.getOrderDetails(orderNumber).subscribe({
      next: (order) => {
        this.order.set(order);
        this.isLoading.set(false);
      },
      error: () => {
        this.error.set('Unable to load this order. Check My Orders for the latest information.');
        this.isLoading.set(false);
      },
    });
  }
}
