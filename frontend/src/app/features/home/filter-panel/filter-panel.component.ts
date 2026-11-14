import { Component, effect, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BrandSummary } from '../../../core/brands/brands.models';
import { ProductFilterValues } from '../../../core/products/products.models';

@Component({
  selector: 'app-filter-panel',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './filter-panel.component.html',
  styleUrl: './filter-panel.component.scss',
})
export class FilterPanelComponent {
  readonly brands = input<BrandSummary[]>([]);
  readonly brandsLoading = input(false);
  readonly filters = input.required<ProductFilterValues>();
  readonly isBusy = input(false);
  readonly brandsError = input('');
  readonly applyFilters = output<ProductFilterValues>();
  readonly clearFilters = output<void>();
  readonly retryBrands = output<void>();

  minPrice: string | number | null = '';
  maxPrice: string | number | null = '';
  brandSearch = '';
  stockStatus: ProductFilterValues['stockStatus'] = '';
  selectedBrandIds: number[] = [];

  constructor() {
    effect(() => {
      const filters = this.filters();
      this.minPrice = filters.minPrice;
      this.maxPrice = filters.maxPrice;
      this.stockStatus = filters.stockStatus;
      this.selectedBrandIds = [...filters.brandIds];
    });
  }

  readonly visibleBrands = (): BrandSummary[] => {
    const term = this.brandSearch.trim().toLocaleLowerCase();
    return term
      ? this.brands().filter((brand) => brand.nameEn.toLocaleLowerCase().includes(term))
      : this.brands();
  };

  toggleBrand(brandId: number, event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    this.selectedBrandIds = checked
      ? [...this.selectedBrandIds, brandId]
      : this.selectedBrandIds.filter((selectedId) => selectedId !== brandId);
  }

  apply(): void {
    this.applyFilters.emit({
      minPrice: this.normalizePrice(this.minPrice),
      maxPrice: this.normalizePrice(this.maxPrice),
      brandIds: this.selectedBrandIds,
      stockStatus: this.stockStatus,
    });
  }

  clear(): void {
    this.clearFilters.emit();
  }

  private normalizePrice(value: string | number | null): string {
    if (typeof value === 'string') {
      return value.trim();
    }

    return value === null ? '' : String(value);
  }
}
