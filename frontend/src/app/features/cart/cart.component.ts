import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { Cart, CheckoutLineError } from '../../core/cart/cart.models';
import { CartService } from '../../core/cart/cart.service';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [DecimalPipe, RouterLink],
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.scss',
})
export class CartComponent implements OnInit {
  private readonly cartService = inject(CartService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly cart = signal<Cart | null>(null);
  readonly isLoading = signal(true);
  readonly isCheckingOut = signal(false);
  readonly busyProductId = signal<number | null>(null);
  readonly pageError = signal('');
  readonly checkoutErrors = signal<Record<number, string>>({});

  ngOnInit(): void {
    this.loadCart();
  }

  updateQuantity(productId: number, event: Event): void {
    const quantity = Number((event.target as HTMLInputElement).value);
    if (!Number.isSafeInteger(quantity) || quantity < 1) {
      this.setLineError(productId, 'Quantity must be at least 1.');
      this.loadCart();
      return;
    }

    this.busyProductId.set(productId);
    this.checkoutErrors.update((errors) => {
      const updated = { ...errors };
      delete updated[productId];
      return updated;
    });
    this.cartService.updateItem(productId, quantity)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (cart) => {
          this.cart.set(cart);
          this.busyProductId.set(null);
        },
        error: (error: unknown) => {
          this.setLineError(productId, this.getErrorMessage(error));
          this.busyProductId.set(null);
          this.loadCart();
        },
      });
  }

  removeItem(productId: number): void {
    this.busyProductId.set(productId);
    this.cartService.removeItem(productId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (cart) => {
          this.cart.set(cart);
          this.busyProductId.set(null);
          this.checkoutErrors.update((errors) => {
            const updated = { ...errors };
            delete updated[productId];
            return updated;
          });
        },
        error: (error: unknown) => {
          this.setLineError(productId, this.getErrorMessage(error));
          this.busyProductId.set(null);
        },
      });
  }

  checkout(): void {
    if (this.isCheckingOut() || !this.cart()?.items.length) {
      return;
    }

    this.isCheckingOut.set(true);
    this.pageError.set('');
    this.checkoutErrors.set({});
    this.cartService.checkout()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.isCheckingOut.set(false);
          if (result.isValid && result.items && result.subtotal !== null) {
            void this.router.navigate(['/checkout'], {
              state: { checkout: result },
            });
            return;
          }

          this.setCheckoutErrors(result.errors ?? []);
          this.loadCart();
        },
        error: (error: unknown) => {
          this.pageError.set(this.getErrorMessage(error));
          this.isCheckingOut.set(false);
        },
      });
  }

  retry(): void {
    this.loadCart();
  }

  private loadCart(): void {
    this.isLoading.set(true);
    this.pageError.set('');
    this.cartService.getCart()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (cart) => {
          this.cart.set(cart);
          this.isLoading.set(false);
        },
        error: (error: unknown) => {
          this.pageError.set(this.getErrorMessage(error));
          this.isLoading.set(false);
        },
      });
  }

  private setCheckoutErrors(errors: CheckoutLineError[]): void {
    this.checkoutErrors.set(
      Object.fromEntries(errors.map((error) => [error.productId, error.reason])),
    );
  }

  private setLineError(productId: number, message: string): void {
    this.checkoutErrors.update((errors) => ({ ...errors, [productId]: message }));
  }

  private getErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      const responseError: unknown = error.error;
      if (typeof responseError === 'object' && responseError !== null && 'error' in responseError) {
        const apiError = responseError.error;
        if (typeof apiError === 'object' && apiError !== null &&
          'message' in apiError && typeof apiError.message === 'string') {
          return apiError.message;
        }
      }
    }

    return 'The cart could not be updated. Please try again.';
  }
}
