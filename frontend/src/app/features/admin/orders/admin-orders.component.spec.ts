import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, Router } from '@angular/router';
import { BehaviorSubject, of } from 'rxjs';
import { AdminOrdersService } from '../../../core/orders/admin-orders.service';
import { AdminOrderListResult } from '../../../core/orders/admin-orders.models';
import { AdminOrdersComponent } from './admin-orders.component';

describe('AdminOrdersComponent', () => {
  let fixture: ComponentFixture<AdminOrdersComponent>;
  let component: AdminOrdersComponent;
  let getOrders: ReturnType<typeof vi.fn>;
  let navigate: ReturnType<typeof vi.fn>;

  const result: AdminOrderListResult = {
    items: [
      {
        id: 7,
        orderNumber: 'ORD-2026-000007',
        createdAt: '2026-11-15T10:00:00Z',
        customerName: 'Test Customer',
        itemsCount: 2,
        total: 1250,
        status: 'Pending',
        paymentStatus: 'Pending',
      },
    ],
    totalCount: 1,
    counts: {
      all: 1,
      pending: 1,
      confirmed: 0,
      processing: 0,
      shipped: 0,
      delivered: 0,
      cancelled: 0,
    },
  };

  beforeEach(async () => {
    getOrders = vi.fn().mockReturnValue(of(result));
    navigate = vi.fn().mockResolvedValue(true);
    const queryParamMap = new BehaviorSubject(convertToParamMap({}));

    await TestBed.configureTestingModule({
      imports: [AdminOrdersComponent],
      providers: [
        { provide: AdminOrdersService, useValue: { getOrders } },
        { provide: ActivatedRoute, useValue: { queryParamMap, snapshot: { queryParamMap: queryParamMap.value } } },
        { provide: Router, useValue: { navigate } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AdminOrdersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('loads and displays orders and status counts', () => {
    expect(getOrders).toHaveBeenCalledWith('All', '', 1, 20);
    expect(component.orders()).toEqual(result.items);
    expect(component.countFor('Pending')).toBe(1);

    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('ORD-2026-000007');
    expect(fixture.nativeElement.textContent).toContain('Test Customer');
  });

  it('puts the selected status and search term in the query string', () => {
    component.searchText.set('  ORD-2026  ');

    component.search();
    expect(navigate).toHaveBeenCalledWith([], {
      relativeTo: TestBed.inject(ActivatedRoute),
      queryParams: { search: 'ORD-2026', page: 1 },
      queryParamsHandling: 'merge',
    });

    component.selectStatus('Shipped');
    expect(navigate).toHaveBeenLastCalledWith([], {
      relativeTo: TestBed.inject(ActivatedRoute),
      queryParams: { status: 'Shipped', page: 1 },
      queryParamsHandling: 'merge',
    });
  });
});
