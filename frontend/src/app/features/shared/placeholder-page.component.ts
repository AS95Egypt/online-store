import { AsyncPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-placeholder-page',
  standalone: true,
  imports: [AsyncPipe],
  template: `
    @if (routeData$ | async; as routeData) {
      <section class="placeholder-page">
        <h1>{{ routeData['title'] }}</h1>
        <p>{{ routeData['message'] }}</p>
      </section>
    }
  `,
  styles: `
    .placeholder-page {
      display: grid;
      min-height: 60vh;
      align-content: center;
      justify-items: center;
      text-align: center;
      color: var(--muted);
    }

    h1 {
      margin: 0 0 0.5rem;
      color: var(--text);
    }
  `,
})
export class PlaceholderPageComponent {
  private readonly route = inject(ActivatedRoute);

  readonly routeData$ = this.route.data;
}
