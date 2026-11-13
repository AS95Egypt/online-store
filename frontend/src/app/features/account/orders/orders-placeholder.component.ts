import { Component } from '@angular/core';

@Component({
  selector: 'app-orders-placeholder',
  standalone: true,
  template: '<section class="placeholder"><p>Orders are coming soon.</p></section>',
  styles: `
    .placeholder {
      display: grid;
      min-height: 60vh;
      place-items: center;
      color: var(--muted);
    }
  `,
})
export class OrdersPlaceholderComponent {}
