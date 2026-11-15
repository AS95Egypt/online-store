import { DecimalPipe } from '@angular/common';
import { Component, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CartService } from '../../core/cart/cart.service';
import { AuthService } from '../../core/auth/auth.service';
import { ProductListItem } from '../../core/products/products.models';
import { NotificationService } from '../../core/notifications/notification.service';

@Component({
  selector: 'app-product-card',
  standalone: true,
  imports: [DecimalPipe, RouterLink],
  templateUrl: './product-card.component.html',
  styleUrl: './product-card.component.scss',
})
export class ProductCardComponent {
  private readonly authService = inject(AuthService);
  private readonly cartService = inject(CartService);
  private readonly notificationService = inject(NotificationService);
  private readonly router = inject(Router);

  readonly product = input.required<ProductListItem>();
  readonly isAdding = signal(false);

  addToCart(event: Event): void {
    event.preventDefault();
    event.stopPropagation();

    if (!this.authService.isAuthenticated()) {
      void this.router.navigate(['/login']);
      return;
    }

    if (this.isAdding()) {
      return;
    }

    this.isAdding.set(true);
    this.cartService.addItem(this.product().id).subscribe({
      next: () => this.notificationService.show('Added to your cart.'),
      error: (error: unknown) => {
        this.isAdding.set(false);
        this.notificationService.show(this.getErrorMessage(error));
      },
      complete: () => this.isAdding.set(false),
    });
  }

  private getErrorMessage(error: unknown): string {
    if (typeof error === 'object' && error !== null && 'error' in error) {
      const response = error.error;
      if (typeof response === 'object' && response !== null && 'error' in response) {
        const apiError = response.error;
        if (typeof apiError === 'object' && apiError !== null && 'message' in apiError &&
          typeof apiError.message === 'string') {
          return apiError.message;
        }
      }
    }

    return 'The product could not be added to your cart.';
  }
}
