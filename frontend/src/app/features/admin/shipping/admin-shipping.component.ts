import { CommonModule, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { AdminShippingService } from '../../../core/shipping/admin-shipping.service';
import { AdminGovernorate } from '../../../core/shipping/admin-shipping.models';
import { NotificationService } from '../../../core/notifications/notification.service';
import { readAdminApiError } from '../../../core/admin/admin-api-errors';

@Component({
  selector: 'app-admin-shipping',
  standalone: true,
  imports: [CommonModule, DecimalPipe],
  templateUrl: './admin-shipping.component.html',
  styleUrl: './admin-shipping.component.scss',
})
export class AdminShippingComponent implements OnInit {
  private readonly shippingService = inject(AdminShippingService);
  private readonly notificationService = inject(NotificationService);

  readonly governorates = signal<AdminGovernorate[]>([]);
  readonly isLoading = signal(true);
  readonly loadError = signal('');
  readonly editingId = signal<number | null>(null);
  readonly editingFee = signal('');
  readonly fieldError = signal('');
  readonly savingId = signal<number | null>(null);

  ngOnInit(): void {
    this.loadGovernorates();
  }

  loadGovernorates(): void {
    this.isLoading.set(true);
    this.loadError.set('');
    this.shippingService.getGovernorates().subscribe({
      next: (governorates) => {
        this.governorates.set(governorates);
        this.isLoading.set(false);
      },
      error: () => {
        this.loadError.set('Shipping fees could not be loaded. Please try again.');
        this.isLoading.set(false);
      },
    });
  }

  startEdit(governorate: AdminGovernorate): void {
    this.editingId.set(governorate.id);
    this.editingFee.set(governorate.shippingFee.toFixed(2));
    this.fieldError.set('');
  }

  updateFee(event: Event): void {
    this.editingFee.set((event.target as HTMLInputElement).value);
    this.fieldError.set('');
  }

  cancelEdit(): void {
    if (this.savingId() === null) {
      this.editingId.set(null);
      this.fieldError.set('');
    }
  }

  saveFee(governorate: AdminGovernorate): void {
    const feeText = this.editingFee().trim();
    const shippingFee = Number(feeText);
    if (!feeText || !Number.isFinite(shippingFee) || shippingFee < 0 || shippingFee > Number.MAX_SAFE_INTEGER) {
      this.fieldError.set('Enter a non-negative shipping fee.');
      return;
    }

    this.fieldError.set('');
    this.savingId.set(governorate.id);
    this.shippingService.updateFee(governorate.id, { shippingFee }).subscribe({
      next: (updated) => {
        this.governorates.update((items) =>
          items.map((item) => item.id === updated.id ? updated : item),
        );
        this.editingId.set(null);
        this.savingId.set(null);
        this.notificationService.show(`${updated.name} shipping fee updated.`);
      },
      error: (error: unknown) => {
        const parsed = readAdminApiError(error, 'Unable to update this shipping fee.');
        this.fieldError.set(parsed.fields['shippingfee'] ?? parsed.message);
        this.savingId.set(null);
      },
    });
  }
}
