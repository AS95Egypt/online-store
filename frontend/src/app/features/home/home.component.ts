import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ProductListItem, ProductPageMeta } from '../../core/products/products.models';
import { ProductsService } from '../../core/products/products.service';
import { ProductCardComponent } from '../../shared/product-card/product-card.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [ProductCardComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
})
export class HomeComponent implements OnInit {
  private readonly productsService = inject(ProductsService);
  private readonly pageSize = 20;

  readonly products = signal<ProductListItem[]>([]);
  readonly pageMeta = signal<ProductPageMeta | null>(null);
  readonly isLoading = signal(true);
  readonly isLoadingMore = signal(false);
  readonly loadError = signal('');
  readonly loadMoreError = signal('');
  readonly hasMore = computed(() => {
    const meta = this.pageMeta();
    return meta !== null && meta.page * meta.pageSize < meta.total;
  });
  readonly skeletonCards = Array.from({ length: 8 }, (_, index) => index);

  ngOnInit(): void {
    this.loadPage(1);
  }

  loadMore(): void {
    const meta = this.pageMeta();
    if (!meta || !this.hasMore() || this.isLoadingMore()) {
      return;
    }

    this.loadPage(meta.page + 1);
  }

  retry(): void {
    const meta = this.pageMeta();
    this.loadPage(this.products().length === 0 || !meta ? 1 : meta.page + 1);
  }

  private loadPage(page: number): void {
    const isFirstPage = page === 1 && this.products().length === 0;
    if (isFirstPage) {
      this.isLoading.set(true);
      this.loadError.set('');
    } else {
      this.isLoadingMore.set(true);
      this.loadMoreError.set('');
    }

    this.productsService.getProducts(page, this.pageSize).subscribe({
      next: (result) => {
        this.products.update((current) =>
          page === 1 ? result.data : [...current, ...result.data],
        );
        this.pageMeta.set(result.meta);
        this.isLoading.set(false);
        this.isLoadingMore.set(false);
      },
      error: () => {
        if (isFirstPage) {
          this.loadError.set('Products could not be loaded. Please try again.');
        } else {
          this.loadMoreError.set('More products could not be loaded. Please try again.');
        }
        this.isLoading.set(false);
        this.isLoadingMore.set(false);
      },
    });
  }
}
