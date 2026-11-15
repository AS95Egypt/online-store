import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { NotificationService } from '../../../core/notifications/notification.service';
import { OrdersService } from '../../../core/orders/orders.service';
import { OrderDetails, OrderSummary } from '../../../core/orders/orders.models';

@Component({
  selector: 'app-my-orders',
  standalone: true,
  imports: [DatePipe, DecimalPipe],
  templateUrl: './my-orders.component.html',
  styleUrl: './my-orders.component.scss',
})
export class MyOrdersComponent implements OnInit {
  private readonly ordersService = inject(OrdersService);
  private readonly notificationService = inject(NotificationService);

  readonly orders = signal<OrderSummary[]>([]);
  readonly orderDetails = signal<Record<string, OrderDetails>>({});
  readonly expandedOrderNumber = signal<string | null>(null);
  readonly confirmingCancellation = signal<string | null>(null);
  readonly cancellingOrderNumbers = signal<Set<string>>(new Set());
  readonly loadingDetails = signal<string | null>(null);
  readonly detailsErrors = signal<Record<string, string>>({});
  readonly isLoading = signal(true);
  readonly pageError = signal('');
  readonly statuses = ['Pending', 'Confirmed', 'Processing', 'Shipped', 'Delivered'];

  ngOnInit(): void {
    this.loadOrders();
  }

  toggleOrder(orderNumber: string): void {
    if (this.expandedOrderNumber() === orderNumber) {
      this.expandedOrderNumber.set(null);
      return;
    }

    this.expandedOrderNumber.set(orderNumber);
    if (this.orderDetails()[orderNumber] || this.loadingDetails() === orderNumber) {
      return;
    }

    this.loadingDetails.set(orderNumber);
    this.ordersService.getOrderDetails(orderNumber).subscribe({
      next: (details) => {
        this.orderDetails.update((current) => ({ ...current, [orderNumber]: details }));
        this.loadingDetails.set(null);
      },
      error: (error: unknown) => {
        this.detailsErrors.update((current) => ({
          ...current,
          [orderNumber]: this.getErrorMessage(error, 'Unable to load order details.'),
        }));
        this.loadingDetails.set(null);
      },
    });
  }

  retryDetails(orderNumber: string): void {
    this.detailsErrors.update((current) => {
      const updated = { ...current };
      delete updated[orderNumber];
      return updated;
    });
    this.expandedOrderNumber.set(null);
    this.toggleOrder(orderNumber);
  }

  requestCancellation(orderNumber: string): void {
    if (this.orderDetails()[orderNumber]?.status !== 'Pending' || this.isCancelling(orderNumber)) {
      return;
    }

    this.confirmingCancellation.set(orderNumber);
  }

  keepOrder(): void {
    this.confirmingCancellation.set(null);
  }

  isCancelling(orderNumber: string): boolean {
    return this.cancellingOrderNumbers().has(orderNumber);
  }

  confirmCancellation(orderNumber: string): void {
    if (this.confirmingCancellation() !== orderNumber || this.isCancelling(orderNumber)) {
      return;
    }

    this.setCancelling(orderNumber, true);
    this.ordersService.cancelOrder(orderNumber).subscribe({
      next: (details) => {
        this.updateOrderDetails(orderNumber, details);
        this.confirmingCancellation.set(null);
        this.setCancelling(orderNumber, false);
        this.notificationService.show('Order cancelled.');
      },
      error: (error: unknown) => {
        this.setCancelling(orderNumber, false);
        if (error instanceof HttpErrorResponse && error.status === 409) {
          this.confirmingCancellation.set(null);
          this.notificationService.show('This order can no longer be cancelled.');
          this.refreshOrderDetails(orderNumber);
          return;
        }

        this.notificationService.show('Unable to cancel this order. Please try again.');
      },
    });
  }

  isStatusComplete(currentStatus: string, status: string): boolean {
    if (currentStatus === 'Cancelled') {
      return false;
    }
    return this.statuses.indexOf(status) <= this.statuses.indexOf(currentStatus);
  }

  loadOrders(): void {
    this.isLoading.set(true);
    this.pageError.set('');
    this.ordersService.getOrders().subscribe({
      next: (orders) => {
        this.orders.set(orders);
        this.isLoading.set(false);
      },
      error: (error: unknown) => {
        this.pageError.set(this.getErrorMessage(error, 'Unable to load your orders.'));
        this.isLoading.set(false);
      },
    });
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    if (error instanceof HttpErrorResponse) {
      const body = error.error as { message?: unknown; error?: { message?: unknown }; title?: unknown } | null;
      for (const candidate of [body?.error?.message, body?.message, body?.title]) {
        if (typeof candidate === 'string' && candidate.trim()) {
          return candidate;
        }
      }
    }
    return fallback;
  }

  private refreshOrderDetails(orderNumber: string): void {
    this.detailsErrors.update((current) => {
      const updated = { ...current };
      delete updated[orderNumber];
      return updated;
    });
    this.loadingDetails.set(orderNumber);
    this.ordersService.getOrderDetails(orderNumber).subscribe({
      next: (details) => {
        this.updateOrderDetails(orderNumber, details);
        this.loadingDetails.set(null);
      },
      error: (error: unknown) => {
        this.detailsErrors.update((current) => ({
          ...current,
          [orderNumber]: this.getErrorMessage(error, 'Unable to refresh this order.'),
        }));
        this.loadingDetails.set(null);
      },
    });
  }

  private updateOrderDetails(orderNumber: string, details: OrderDetails): void {
    this.orderDetails.update((current) => ({ ...current, [orderNumber]: details }));
    this.orders.update((current) =>
      current.map((order) => order.orderNumber === orderNumber
        ? { ...order, status: details.status }
        : order),
    );
  }

  private setCancelling(orderNumber: string, isCancelling: boolean): void {
    this.cancellingOrderNumbers.update((current) => {
      const updated = new Set(current);
      if (isCancelling) {
        updated.add(orderNumber);
      } else {
        updated.delete(orderNumber);
      }
      return updated;
    });
  }
}
