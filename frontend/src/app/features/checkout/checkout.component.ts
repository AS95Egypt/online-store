import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { forkJoin, switchMap } from 'rxjs';
import { AccountService } from '../../core/account/account.service';
import { Cart, CheckoutCalculationResult } from '../../core/cart/cart.models';
import { CartService } from '../../core/cart/cart.service';
import { OrdersService } from '../../core/orders/orders.service';
import { OrderItemError } from '../../core/orders/orders.models';
import { ShippingService } from '../../core/shipping/shipping.service';
import { Governorate } from '../../core/shipping/shipping.models';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [DecimalPipe, ReactiveFormsModule, RouterLink],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.scss',
})
export class CheckoutComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly accountService = inject(AccountService);
  private readonly cartService = inject(CartService);
  private readonly ordersService = inject(OrdersService);
  private readonly shippingService = inject(ShippingService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly form = this.fb.nonNullable.group({
    fullName: ['', [Validators.required, Validators.maxLength(200)]],
    phone: ['', [Validators.required, Validators.maxLength(30)]],
    governorateId: [0, [Validators.required, Validators.min(1)]],
    city: ['', [Validators.required, Validators.maxLength(150)]],
    district: ['', [Validators.required, Validators.maxLength(150)]],
    street: ['', [Validators.required, Validators.maxLength(250)]],
    buildingNumber: ['', [Validators.required, Validators.maxLength(50)]],
    apartment: ['', [Validators.maxLength(100)]],
    instructions: ['', [Validators.maxLength(1000)]],
  });

  readonly cart = signal<Cart | null>(null);
  readonly governorates = signal<Governorate[]>([]);
  readonly calculation = signal<CheckoutCalculationResult | null>(null);
  readonly isLoading = signal(true);
  readonly isCalculating = signal(false);
  readonly isSubmitting = signal(false);
  readonly loadError = signal('');
  readonly pageError = signal('');
  readonly fieldErrors = signal<Record<string, string>>({});
  readonly itemErrors = signal<Record<number, string>>({});
  private readonly clientRequestId = globalThis.crypto.randomUUID();

  ngOnInit(): void {
    this.loadCheckout();
  }

  retryLoad(): void {
    this.loadCheckout();
  }

  private loadCheckout(): void {
    this.isLoading.set(true);
    this.loadError.set('');
    forkJoin({
      cart: this.cartService.getCart(),
      address: this.accountService.getAddress(),
      governorates: this.shippingService.getOptions(),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ cart, address, governorates }) => {
          this.cart.set(cart);
          this.governorates.set(governorates);
          this.form.patchValue({
            fullName: address.fullName ?? '',
            phone: address.phone ?? '',
            governorateId: address.governorateId ?? 0,
            city: address.city ?? '',
            district: address.district ?? '',
            street: address.street ?? '',
            buildingNumber: address.buildingNumber ?? '',
            apartment: address.apartment ?? '',
            instructions: address.instructions ?? '',
          });
          this.isLoading.set(false);
          if (cart.items.length === 0) {
            return;
          }

          if (address.governorateId) {
            this.refreshCalculation(address.governorateId);
          } else {
            this.refreshCalculation();
          }
        },
        error: (error: unknown) => {
          this.loadError.set(this.getErrorMessage(error, 'Unable to load checkout.'));
          this.isLoading.set(false);
        },
      });
  }

  onGovernorateChanged(): void {
    const governorateId = this.form.controls.governorateId.value;
    const updatedFieldErrors = { ...this.fieldErrors() };
    delete updatedFieldErrors['governorateId'];
    this.fieldErrors.set(updatedFieldErrors);
    this.refreshCalculation(governorateId > 0 ? governorateId : undefined);
  }

  placeOrder(): void {
    if (this.isSubmitting()) {
      return;
    }

    this.form.markAllAsTouched();
    if (this.form.invalid) {
      return;
    }
    if (!this.cart()?.items.length) {
      this.pageError.set('Your cart is empty.');
      return;
    }
    const calculation = this.calculation();
    if (!calculation?.isValid || calculation.total === null) {
      this.pageError.set(calculation?.governorateError ?? 'Review the cart before placing your order.');
      return;
    }

    this.pageError.set('');
    this.fieldErrors.set({});
    this.itemErrors.set({});
    this.isSubmitting.set(true);
    this.accountService.updateAddress(this.form.getRawValue())
      .pipe(
        switchMap(() => this.ordersService.createOrder(this.clientRequestId, calculation)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (order) => {
          this.isSubmitting.set(false);
          this.cartService.clearItemCount();
          void this.router.navigate(['/order-confirmation', order.orderNumber]);
        },
        error: (error: unknown) => {
          this.isSubmitting.set(false);
          this.handleSubmitError(error);
        },
      });
  }

  private refreshCalculation(governorateId?: number, preserveSubmitErrors = false): void {
    this.isCalculating.set(true);
    this.calculation.set(null);
    if (!preserveSubmitErrors) {
      this.pageError.set('');
      this.itemErrors.set({});
    }
    this.cartService.checkout(governorateId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.calculation.set(result);
          this.isCalculating.set(false);
          if (result.errors?.length || !preserveSubmitErrors) {
            this.itemErrors.set(result.errors?.length ? this.toItemErrorMap(result.errors) : {});
          }
        },
        error: (error: unknown) => {
          this.pageError.set(this.getErrorMessage(error, 'Unable to calculate checkout totals.'));
          this.isCalculating.set(false);
        },
      });
  }

  private handleSubmitError(error: unknown): void {
    if (!(error instanceof HttpErrorResponse)) {
      this.pageError.set('Unable to place your order. Please try again.');
      return;
    }

    const body = this.asRecord(error.error);
    const fieldErrors = this.readFieldErrors(body);
    if (Object.keys(fieldErrors).length > 0) {
      this.fieldErrors.set(fieldErrors);
      return;
    }

    const errorEnvelope = this.asRecord(body?.['error']);
    const details = errorEnvelope?.['details'];
    const errorCode = errorEnvelope?.['code'];
    if (Array.isArray(details)) {
      const itemErrors = details.filter(this.isOrderItemError);
      if (itemErrors.length > 0) {
        this.itemErrors.set(this.toItemErrorMap(itemErrors));
        this.pageError.set(
          this.getErrorMessage(error, 'The cart changed. Review the updated order summary.'),
        );
        this.refreshCalculation(this.form.controls.governorateId.value, true);
        return;
      }
    }
    if (errorCode === 'checkout_changed') {
      this.pageError.set(
        this.getErrorMessage(error, 'The order summary changed. Review the updated totals.'),
      );
      this.refreshCalculation(this.form.controls.governorateId.value, true);
      return;
    }

    this.pageError.set(
      this.getErrorMessage(error, 'Unable to place your order. Please try again.'),
    );
  }

  private readFieldErrors(body: Record<string, unknown> | null): Record<string, string> {
    const errors = this.asRecord(body?.['errors']);
    if (!errors) {
      return {};
    }

    const mapped: Record<string, string> = {};
    for (const [field, values] of Object.entries(errors)) {
      const control = field.charAt(0).toLowerCase() + field.slice(1);
      if (Array.isArray(values)) {
        const message = values.find((value): value is string => typeof value === 'string');
        if (message) {
          mapped[control] = message;
        }
      } else if (typeof values === 'string') {
        mapped[control] = values;
      }
    }
    return mapped;
  }

  private toItemErrorMap(errors: OrderItemError[]): Record<number, string> {
    return Object.fromEntries(errors.map((error) => [error.productId, error.reason]));
  }

  private isOrderItemError(value: unknown): value is OrderItemError {
    const item = this.asRecord(value);
    return typeof item?.['productId'] === 'number' && typeof item['reason'] === 'string';
  }

  private asRecord(value: unknown): Record<string, unknown> | null {
    return typeof value === 'object' && value !== null
      ? value as Record<string, unknown>
      : null;
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    if (error instanceof HttpErrorResponse) {
      const body = this.asRecord(error.error);
      const envelope = this.asRecord(body?.['error']);
      for (const candidate of [envelope?.['message'], body?.['message'], body?.['detail'], body?.['title']]) {
        if (typeof candidate === 'string' && candidate.trim()) {
          return candidate;
        }
      }
    }
    return fallback;
  }
}
