import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AdminOrdersService } from '../../../core/orders/admin-orders.service';
import {
  ADMIN_ORDER_FILTERS,
  AdminOrderListItem,
  AdminOrderListResult,
  OrderStatusCounts,
} from '../../../core/orders/admin-orders.models';

const EMPTY_COUNTS: OrderStatusCounts = {
  all: 0,
  pending: 0,
  confirmed: 0,
  processing: 0,
  shipped: 0,
  delivered: 0,
  cancelled: 0,
};

@Component({
  selector: 'app-admin-orders',
  standalone: true,
  imports: [CommonModule, DatePipe, DecimalPipe],
  templateUrl: './admin-orders.component.html',
  styleUrl: './admin-orders.component.scss',
})
export class AdminOrdersComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly ordersService = inject(AdminOrdersService);

  readonly statuses = ADMIN_ORDER_FILTERS;
  readonly skeletonRows = [1, 2, 3, 4, 5];
  readonly orders = signal<AdminOrderListItem[]>([]);
  readonly counts = signal<OrderStatusCounts>(EMPTY_COUNTS);
  readonly totalCount = signal(0);
  readonly page = signal(1);
  readonly pageSize = 20;
  readonly status = signal('All');
  readonly searchText = signal('');
  readonly isLoading = signal(true);
  readonly loadError = signal('');

  ngOnInit(): void {
    this.route.queryParamMap.subscribe((params) => {
      const requestedStatus = params.get('status') ?? 'All';
      this.status.set(
        requestedStatus === 'All' || this.statuses.includes(requestedStatus as (typeof this.statuses)[number])
          ? requestedStatus
          : 'All',
      );
      this.searchText.set(params.get('search') ?? '');
      const requestedPage = Number(params.get('page') ?? '1');
      this.page.set(Number.isInteger(requestedPage) && requestedPage > 0 ? requestedPage : 1);
      this.loadOrders();
    });
  }

  loadOrders(): void {
    this.isLoading.set(true);
    this.loadError.set('');
    this.ordersService.getOrders(this.status(), this.searchText(), this.page(), this.pageSize).subscribe({
      next: (result: AdminOrderListResult) => {
        this.orders.set(result.items);
        this.counts.set(result.counts);
        this.totalCount.set(result.totalCount);
        this.isLoading.set(false);
      },
      error: () => {
        this.loadError.set('Orders could not be loaded. Please try again.');
        this.isLoading.set(false);
      },
    });
  }

  updateSearch(event: Event): void {
    this.searchText.set((event.target as HTMLInputElement).value);
  }

  search(): void {
    this.updateQuery({ search: this.searchText().trim() || null, page: 1 });
  }

  selectStatus(status: string): void {
    this.updateQuery({ status: status === 'All' ? null : status, page: 1 });
  }

  clearFilters(): void {
    this.searchText.set('');
    this.updateQuery({ status: null, search: null, page: 1 });
  }

  changePage(nextPage: number): void {
    if (nextPage < 1 || nextPage > this.pageCount()) {
      return;
    }
    this.updateQuery({ page: nextPage });
  }

  pageCount(): number {
    return Math.max(1, Math.ceil(this.totalCount() / this.pageSize));
  }

  countFor(status: string): number {
    return status === 'All'
      ? this.counts().all
      : this.counts()[status.toLowerCase() as keyof OrderStatusCounts];
  }

  openOrder(order: AdminOrderListItem): void {
    void this.router.navigate(['/admin/orders', order.id]);
  }

  onRowKeydown(event: KeyboardEvent, order: AdminOrderListItem): void {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      this.openOrder(order);
    }
  }

  private updateQuery(queryParams: Record<string, string | number | null>): void {
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams,
      queryParamsHandling: 'merge',
    });
  }
}
