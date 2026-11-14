import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { BrandSummary } from '../../core/brands/brands.models';
import { BrandsService } from '../../core/brands/brands.service';
import {
  ProductFilterValues,
  ProductListFilters,
  ProductListItem,
  ProductPageMeta,
} from '../../core/products/products.models';
import { ProductsService } from '../../core/products/products.service';
import { FilterPanelComponent } from './filter-panel/filter-panel.component';
import { ProductCardComponent } from '../../shared/product-card/product-card.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [FilterPanelComponent, ProductCardComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
})
export class HomeComponent implements OnInit {
  private readonly productsService = inject(ProductsService);
  private readonly brandsService = inject(BrandsService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly pageSize = 20;
  private activeQueryKey: string | null = null;
  private activeFilterKey: string | null = null;
  private requestGeneration = 0;
  private requestFilters: ProductListFilters = {};

  readonly products = signal<ProductListItem[]>([]);
  readonly brands = signal<BrandSummary[]>([]);
  readonly isBrandsLoading = signal(true);
  readonly pageMeta = signal<ProductPageMeta | null>(null);
  readonly activePage = signal(1);
  readonly requestedPage = signal(1);
  readonly isLoading = signal(true);
  readonly isLoadingMore = signal(false);
  readonly loadError = signal('');
  readonly loadMoreError = signal('');
  readonly brandsError = signal('');
  readonly search = signal('');
  readonly categoryId = signal('');
  readonly sort = signal('');
  readonly filterValues = signal<ProductFilterValues>({
    minPrice: '',
    maxPrice: '',
    brandIds: [],
    stockStatus: '',
  });
  readonly hasMore = computed(() => {
    const meta = this.pageMeta();
    return meta !== null && meta.page * meta.pageSize < meta.total;
  });
  readonly hasActiveFilters = computed(() =>
    Boolean(
      this.search().trim() ||
      this.categoryId() ||
      this.filterValues().minPrice ||
      this.filterValues().maxPrice ||
      this.filterValues().brandIds.length ||
      this.filterValues().stockStatus,
    ),
  );
  readonly skeletonCards = Array.from({ length: 8 }, (_, index) => index);

  ngOnInit(): void {
    this.route.queryParamMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((params) => this.onQueryParamsChanged(params));
    this.loadBrands();
  }

  loadMore(): void {
    if (!this.hasMore() || this.isLoadingMore()) {
      return;
    }

    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { page: this.activePage() + 1 },
      queryParamsHandling: 'merge',
    });
  }

  retry(): void {
    if (this.loadMoreError()) {
      this.loadProductsPage(this.requestedPage(), true);
      return;
    }

    this.loadInitialPages(this.requestedPage(), this.requestFilters, this.requestGeneration);
  }

  retryBrands(): void {
    this.loadBrands();
  }

  applyFilters(filters: ProductFilterValues): void {
    const brandIds = filters.brandIds.join(',');
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {
        minPrice: filters.minPrice || null,
        maxPrice: filters.maxPrice || null,
        brandIds: brandIds || null,
        stockStatus: filters.stockStatus || null,
        page: 1,
      },
      queryParamsHandling: 'merge',
    });
  }

  clearFilters(): void {
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {
        minPrice: null,
        maxPrice: null,
        brandIds: null,
        stockStatus: null,
        page: 1,
      },
      queryParamsHandling: 'merge',
    });
  }

  changeSort(sort: string): void {
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { sort: sort || null, page: 1 },
      queryParamsHandling: 'merge',
    });
  }

  changeSortFromEvent(event: Event): void {
    this.changeSort((event.target as HTMLSelectElement).value);
  }

  private onQueryParamsChanged(params: ParamMap): void {
    this.requestFilters = {
      search: params.get('search') ?? undefined,
      categoryId: params.get('categoryId') ?? undefined,
      brandIds: params.get('brandIds') ?? undefined,
      minPrice: params.get('minPrice') ?? undefined,
      maxPrice: params.get('maxPrice') ?? undefined,
      stockStatus: params.get('stockStatus') ?? undefined,
      sort: params.get('sort') ?? undefined,
    };
    const filterKey = [
      this.requestFilters.brandIds ?? '',
      this.requestFilters.minPrice ?? '',
      this.requestFilters.maxPrice ?? '',
      this.requestFilters.stockStatus ?? '',
    ].join('\u001f');
    this.search.set(params.get('search') ?? '');
    this.categoryId.set(params.get('categoryId') ?? '');
    this.sort.set(params.get('sort') ?? '');
    if (filterKey !== this.activeFilterKey) {
      this.activeFilterKey = filterKey;
      this.filterValues.set({
        minPrice: params.get('minPrice') ?? '',
        maxPrice: params.get('maxPrice') ?? '',
        brandIds: (params.get('brandIds') ?? '')
          .split(',')
          .filter((id) => /^\d+$/.test(id))
          .map(Number),
        stockStatus: this.toStockStatus(params.get('stockStatus')),
      });
    }

    const pageValue = Number(params.get('page'));
    const page = Number.isSafeInteger(pageValue) && pageValue > 0 ? pageValue : 1;
    this.requestedPage.set(page);
    const queryKey = [
      this.requestFilters.search ?? '',
      this.requestFilters.categoryId ?? '',
      this.requestFilters.brandIds ?? '',
      this.requestFilters.minPrice ?? '',
      this.requestFilters.maxPrice ?? '',
      this.requestFilters.stockStatus ?? '',
      this.requestFilters.sort ?? '',
    ].join('\u001f');

    if (queryKey !== this.activeQueryKey || page < this.activePage()) {
      this.activeQueryKey = queryKey;
      const requestGeneration = ++this.requestGeneration;
      this.products.set([]);
      this.pageMeta.set(null);
      this.activePage.set(1);
      this.loadInitialPages(page, this.requestFilters, requestGeneration);
      return;
    }

    if (page > this.activePage() + 1) {
      const requestGeneration = ++this.requestGeneration;
      this.products.set([]);
      this.pageMeta.set(null);
      this.activePage.set(1);
      this.loadInitialPages(page, this.requestFilters, requestGeneration);
    } else if (page > this.activePage()) {
      this.loadProductsPage(page, true);
    }
  }

  private loadInitialPages(
    targetPage: number,
    filters: ProductListFilters,
    requestGeneration: number,
  ): void {
    this.isLoading.set(true);
    this.isLoadingMore.set(false);
    this.loadError.set('');
    this.loadMoreError.set('');
    this.productsService.getProducts(1, this.pageSize, filters)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (firstPage) => {
          if (requestGeneration !== this.requestGeneration) {
            return;
          }

          const lastPage = Math.min(targetPage, Math.max(1, Math.ceil(firstPage.meta.total / this.pageSize)));
          if (lastPage === 1) {
            this.completePageLoad([firstPage]);
            return;
          }

          forkJoin(
            Array.from({ length: lastPage - 1 }, (_, index) =>
              this.productsService.getProducts(index + 2, this.pageSize, filters),
            ),
          )
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe({
              next: (remainingPages) => {
                if (requestGeneration === this.requestGeneration) {
                  this.completePageLoad([firstPage, ...remainingPages]);
                }
              },
              error: () => {
                if (requestGeneration !== this.requestGeneration) {
                  return;
                }

                this.loadError.set('Products could not be loaded. Please try again.');
                this.isLoading.set(false);
              },
            });
        },
        error: () => {
          if (requestGeneration !== this.requestGeneration) {
            return;
          }

          this.loadError.set('Products could not be loaded. Please try again.');
          this.isLoading.set(false);
        },
      });
  }

  private loadProductsPage(page: number, append: boolean): void {
    const requestGeneration = this.requestGeneration;
    this.isLoadingMore.set(append);
    if (append) {
      this.loadMoreError.set('');
    }

    this.productsService.getProducts(page, this.pageSize, this.requestFilters)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          if (requestGeneration !== this.requestGeneration) {
            return;
          }

          this.products.update((current) =>
            append ? [...current, ...result.data] : result.data,
          );
          this.pageMeta.set(result.meta);
          this.activePage.set(result.meta.page);
          this.isLoading.set(false);
          this.isLoadingMore.set(false);
          this.loadMoreError.set('');
        },
        error: () => {
          if (requestGeneration !== this.requestGeneration) {
            return;
          }

          if (append) {
            this.loadMoreError.set('More products could not be loaded. Please try again.');
          } else {
            this.loadError.set('Products could not be loaded. Please try again.');
          }
          this.isLoading.set(false);
          this.isLoadingMore.set(false);
        },
      });
  }

  private completePageLoad(results: { data: ProductListItem[]; meta: ProductPageMeta }[]): void {
    this.products.set(results.flatMap((result) => result.data));
    this.pageMeta.set(results[results.length - 1].meta);
    this.activePage.set(results[results.length - 1].meta.page);
    if (this.requestedPage() !== this.activePage()) {
      const page = this.activePage();
      this.requestedPage.set(page);
      void this.router.navigate([], {
        relativeTo: this.route,
        queryParams: { page: page === 1 ? null : page },
        queryParamsHandling: 'merge',
        replaceUrl: true,
      });
    }
    this.isLoading.set(false);
    this.isLoadingMore.set(false);
  }

  private toStockStatus(value: string | null): ProductFilterValues['stockStatus'] {
    return value === 'InStock' || value === 'OutOfStock' ? value : '';
  }

  private loadBrands(): void {
    this.isBrandsLoading.set(true);
    this.brandsError.set('');
    this.brandsService.getBrands()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.brands.set(response.data);
          this.isBrandsLoading.set(false);
        },
        error: () => {
          this.brandsError.set('Brands could not be loaded.');
          this.isBrandsLoading.set(false);
        },
      });
  }
}
