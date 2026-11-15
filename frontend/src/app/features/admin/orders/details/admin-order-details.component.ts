import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { NotificationService } from '../../../../core/notifications/notification.service';
import {
  ADMIN_ORDER_STATUSES,
  AdminOrderDetails,
  AdminOrderStatus,
} from '../../../../core/orders/admin-orders.models';
import { AdminOrdersService } from '../../../../core/orders/admin-orders.service';
import { readAdminApiError } from '../../../../core/admin/admin-api-errors';
import { ConfirmDialogComponent, ConfirmDialogItem } from '../../../../shared/confirm-dialog/confirm-dialog.component';

type ConfirmedAction = 'status' | 'cancel' | 'paid';

@Component({
  selector: 'app-admin-order-details',
  standalone: true,
  imports: [CommonModule, DatePipe, DecimalPipe, RouterLink, ConfirmDialogComponent],
  templateUrl: './admin-order-details.component.html',
  styleUrl: './admin-order-details.component.scss',
})
export class AdminOrderDetailsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly ordersService = inject(AdminOrdersService);
  private readonly notificationService = inject(NotificationService);

  readonly statuses = ADMIN_ORDER_STATUSES;
  readonly order = signal<AdminOrderDetails | null>(null);
  readonly orderId = signal<number | null>(null);
  readonly isLoading = signal(true);
  readonly loadError = signal('');
  readonly isNotFound = signal(false);
  readonly isSubmitting = signal(false);
  readonly actionError = signal('');
  readonly confirmedAction = signal<ConfirmedAction | null>(null);

  get cancellationItems(): ConfirmDialogItem[] {
    return (this.order()?.items ?? []).map((item) => ({
      name: item.productNameSnapshot,
      quantity: item.quantity,
    }));
  }

  get cancellationQuantity(): number {
    return (this.order()?.items ?? []).reduce((total, item) => total + item.quantity, 0);
  }

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      const id = Number(params.get('id'));
      if (!Number.isInteger(id) || id < 1) {
        this.isNotFound.set(true);
        this.isLoading.set(false);
        return;
      }
      this.orderId.set(id);
      this.loadOrder();
    });
  }

  loadOrder(): void {
    const id = this.orderId();
    if (id === null) {
      return;
    }

    this.isLoading.set(true);
    this.loadError.set('');
    this.isNotFound.set(false);
    this.ordersService.getOrderDetails(id).subscribe({
      next: (order) => {
        this.order.set(order);
        this.isLoading.set(false);
      },
      error: (error: unknown) => {
        this.isLoading.set(false);
        if (error instanceof HttpErrorResponse && error.status === 404) {
          this.isNotFound.set(true);
          return;
        }
        this.loadError.set(readAdminApiError(error, 'Order details could not be loaded.').message);
      },
    });
  }

  stepState(status: AdminOrderStatus): 'complete' | 'current' | 'future' {
    const currentStatus = this.order()?.status;
    if (!currentStatus || currentStatus === 'Cancelled') {
      return 'future';
    }
    if (currentStatus === 'Delivered') {
      return 'complete';
    }

    const currentIndex = this.statuses.indexOf(currentStatus);
    const stepIndex = this.statuses.indexOf(status);
    return stepIndex < currentIndex ? 'complete' : stepIndex === currentIndex ? 'current' : 'future';
  }

  nextActionLabel(status: AdminOrderStatus | null): string {
    switch (status) {
      case 'Confirmed':
        return 'Confirm order';
      case 'Processing':
        return 'Start processing';
      case 'Shipped':
        return 'Mark as shipped';
      case 'Delivered':
        return 'Mark as delivered';
      default:
        return '';
    }
  }

  requestAction(action: ConfirmedAction): void {
    if (!this.isSubmitting()) {
      this.confirmedAction.set(action);
      this.actionError.set('');
    }
  }

  dismissConfirmation(): void {
    if (!this.isSubmitting()) {
      this.confirmedAction.set(null);
    }
  }

  confirmAction(): void {
    const order = this.order();
    const action = this.confirmedAction();
    if (!order || !action || this.isSubmitting()) {
      return;
    }

    this.isSubmitting.set(true);
    this.actionError.set('');
    const request = action === 'status'
      ? this.ordersService.updateStatus(order.id, order.availableActions.nextStatus!)
      : action === 'cancel'
        ? this.ordersService.cancelOrder(order.id)
        : this.ordersService.markPaid(order.id);

    request.subscribe({
      next: (updatedOrder) => {
        this.order.set(updatedOrder);
        this.confirmedAction.set(null);
        this.isSubmitting.set(false);
        this.notificationService.show(this.successMessage(action, updatedOrder));
      },
      error: (error: unknown) => {
        this.isSubmitting.set(false);
        if (error instanceof HttpErrorResponse && error.status === 409) {
          this.confirmedAction.set(null);
          this.notificationService.show('This order was just updated.');
          this.loadOrder();
          return;
        }
        this.actionError.set(readAdminApiError(error, 'Unable to update this order.').message);
      },
    });
  }

  get changedCancelledBy(): string {
    const cancellation = this.order()?.statusHistory.find((entry) => entry.status === 'Cancelled');
    return cancellation?.changedBy?.trim() || 'Customer';
  }

  get cancelledAt(): string | null {
    return this.order()?.statusHistory.find((entry) => entry.status === 'Cancelled')?.changedAt ?? null;
  }

  private successMessage(action: ConfirmedAction, order: AdminOrderDetails): string {
    switch (action) {
      case 'status':
        return `Order moved to ${order.status}.`;
      case 'cancel':
        return 'Order cancelled.';
      case 'paid':
        return 'Order marked as paid.';
    }
  }
}
