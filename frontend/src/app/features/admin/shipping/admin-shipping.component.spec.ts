import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { NotificationService } from '../../../core/notifications/notification.service';
import { AdminShippingService } from '../../../core/shipping/admin-shipping.service';
import { AdminGovernorate } from '../../../core/shipping/admin-shipping.models';
import { AdminShippingComponent } from './admin-shipping.component';

describe('AdminShippingComponent', () => {
  let fixture: ComponentFixture<AdminShippingComponent>;
  let component: AdminShippingComponent;
  let updateFee: ReturnType<typeof vi.fn>;
  let showNotification: ReturnType<typeof vi.fn>;

  const governorate: AdminGovernorate = {
    id: 3,
    name: 'Cairo',
    shippingFee: 50,
  };

  beforeEach(async () => {
    updateFee = vi.fn().mockReturnValue(of({ ...governorate, shippingFee: 75.5 }));
    showNotification = vi.fn();

    await TestBed.configureTestingModule({
      imports: [AdminShippingComponent],
      providers: [
        {
          provide: AdminShippingService,
          useValue: {
            getGovernorates: vi.fn().mockReturnValue(of([governorate])),
            updateFee,
          },
        },
        { provide: NotificationService, useValue: { show: showNotification } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AdminShippingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('loads governorate shipping fees', () => {
    expect(component.governorates()).toEqual([governorate]);
    expect(component.isLoading()).toBe(false);
    expect(fixture.nativeElement.textContent).toContain('Changes apply to new orders only.');
  });

  it('rejects a negative fee without calling the API', () => {
    component.startEdit(governorate);
    component.editingFee.set('-1');

    component.saveFee(governorate);

    expect(component.fieldError()).toBe('Enter a non-negative shipping fee.');
    expect(updateFee).not.toHaveBeenCalled();
  });

  it('saves a valid fee and updates the row', () => {
    component.startEdit(governorate);
    component.editingFee.set('75.50');

    component.saveFee(governorate);

    expect(updateFee).toHaveBeenCalledWith(3, { shippingFee: 75.5 });
    expect(component.governorates()[0].shippingFee).toBe(75.5);
    expect(component.editingId()).toBeNull();
    expect(showNotification).toHaveBeenCalledWith('Cairo shipping fee updated.');
  });
});
