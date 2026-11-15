import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CategoriesService } from '../../../core/categories/categories.service';
import { AdminBrandsService } from '../../../core/brands/admin-brands.service';
import { readAdminApiError } from '../../../core/admin/admin-api-errors';
import { NotificationService } from '../../../core/notifications/notification.service';
import {
  AdminProductDetails,
  AdminProductListItem,
  AdminProductRequest,
} from '../../../core/products/admin-products.models';
import { AdminProductsService } from '../../../core/products/admin-products.service';
import { ProductPageMeta } from '../../../core/products/products.models';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-admin-products',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './admin-products.component.html',
  styleUrl: '../admin-catalog.scss',
})
export class AdminProductsComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly productsService = inject(AdminProductsService);
  private readonly categoriesService = inject(CategoriesService);
  private readonly brandsService = inject(AdminBrandsService);
  private readonly notificationService = inject(NotificationService);

  readonly products = signal<AdminProductListItem[]>([]);
  readonly categories = signal<{ id: number; nameEn: string }[]>([]);
  readonly brands = signal<{ id: number; nameEn: string }[]>([]);
  readonly meta = signal<ProductPageMeta>({ total: 0, page: 1, pageSize: 20 });
  readonly page = signal(1);
  readonly searchText = signal('');
  readonly isLoading = signal(true);
  readonly loadError = signal('');
  readonly lookupError = signal('');
  readonly showForm = signal(false);
  readonly editingId = signal<number | null>(null);
  readonly loadingEditor = signal(false);
  readonly isSaving = signal(false);
  readonly changingStatusId = signal<number | null>(null);
  readonly formError = signal('');
  readonly serverErrors = signal<Record<string, string>>({});

  readonly form = this.fb.nonNullable.group({
    nameAr: ['', [Validators.required, Validators.maxLength(250)]],
    nameEn: ['', [Validators.required, Validators.maxLength(250)]],
    descriptionAr: ['', Validators.maxLength(4000)],
    descriptionEn: ['', Validators.maxLength(4000)],
    price: [0, [Validators.required, Validators.min(0.01)]],
    qty: [0, [Validators.required, Validators.min(0)]],
    categoryId: [0, [Validators.required, Validators.min(1)]],
    brandId: [0, [Validators.required, Validators.min(1)]],
    primaryImageUrl: ['', Validators.required],
    thumb1Url: [''],
    thumb2Url: [''],
    thumb3Url: [''],
    thumb4Url: [''],
  });

  ngOnInit(): void {
    this.loadProducts();
    this.loadLookups();
  }

  loadProducts(): void {
    this.isLoading.set(true);
    this.loadError.set('');
    this.productsService.list(this.page(), this.meta().pageSize, this.searchText()).subscribe({
      next: (response) => {
        this.products.set(response.data);
        if (response.meta) {
          this.meta.set(response.meta);
          this.page.set(response.meta.page);
        }
        this.isLoading.set(false);
      },
      error: () => {
        this.loadError.set('Products could not be loaded. Please try again.');
        this.isLoading.set(false);
      },
    });
  }

  search(): void {
    this.page.set(1);
    this.loadProducts();
  }

  updateSearch(event: Event): void {
    this.searchText.set((event.target as HTMLInputElement).value);
  }

  changePage(nextPage: number): void {
    if (nextPage < 1 || nextPage > this.pageCount()) {
      return;
    }
    this.page.set(nextPage);
    this.loadProducts();
  }

  pageCount(): number {
    return Math.max(1, Math.ceil(this.meta().total / this.meta().pageSize));
  }

  startCreate(): void {
    this.editingId.set(null);
    this.form.reset(this.emptyForm());
    this.resetErrors();
    this.showForm.set(true);
  }

  edit(product: AdminProductListItem): void {
    this.loadingEditor.set(true);
    this.showForm.set(false);
    this.resetErrors();
    this.productsService.getById(product.id).subscribe({
      next: (response) => {
        this.editingId.set(product.id);
        this.form.reset(this.formFromProduct(response.data));
        this.showForm.set(true);
        this.loadingEditor.set(false);
      },
      error: () => {
        this.loadingEditor.set(false);
        this.notificationService.show('Product details could not be loaded.');
      },
    });
  }

  cancelEdit(): void {
    if (!this.isSaving()) {
      this.showForm.set(false);
    }
  }

  fieldError(field: string): string {
    return this.serverErrors()[field.toLowerCase()] ?? '';
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.resetErrors();
    this.isSaving.set(true);
    const request = this.toRequest();
    const editingId = this.editingId();
    const saveRequest = editingId === null
      ? this.productsService.create(request)
      : this.productsService.update(editingId, request);
    saveRequest.subscribe({
      next: () => {
        this.showForm.set(false);
        this.isSaving.set(false);
        this.notificationService.show(editingId === null ? 'Product created.' : 'Product updated.');
        if (editingId === null && this.page() !== 1) {
          this.page.set(1);
        }
        this.loadProducts();
      },
      error: (error: unknown) => {
        const parsed = readAdminApiError(error, 'Unable to save this product.');
        this.formError.set(parsed.message);
        this.serverErrors.set(parsed.fields);
        this.isSaving.set(false);
      },
    });
  }

  toggleStatus(product: AdminProductListItem): void {
    const action = product.isActive ? 'deactivate' : 'activate';
    if (!window.confirm(`Are you sure you want to ${action} ${product.nameEn}?`)) {
      return;
    }

    this.changingStatusId.set(product.id);
    this.productsService.setStatus(product.id, !product.isActive).subscribe({
      next: (response) => {
        this.products.update((items) =>
          items.map((item) => item.id === product.id
            ? { ...item, isActive: response.data.isActive }
            : item),
        );
        this.changingStatusId.set(null);
        this.notificationService.show(`Product ${action}d.`);
      },
      error: (error: unknown) => {
        this.changingStatusId.set(null);
        this.notificationService.show(readAdminApiError(error, 'Unable to update product status.').message);
      },
    });
  }

  private loadLookups(): void {
    forkJoin({
      categories: this.categoriesService.getCategories(),
      brands: this.brandsService.list(),
    }).subscribe({
      next: ({ categories, brands }) => {
        this.categories.set(categories.data.map(({ id, nameEn }) => ({ id, nameEn })));
        this.brands.set(brands.data.map(({ id, nameEn }) => ({ id, nameEn })));
      },
      error: (error: HttpErrorResponse) => {
        this.lookupError.set(readAdminApiError(error, 'Categories and brands could not be loaded.').message);
      },
    });
  }

  private toRequest(): AdminProductRequest {
    const value = this.form.getRawValue();
    return {
      nameAr: value.nameAr.trim(),
      nameEn: value.nameEn.trim(),
      descriptionAr: this.optionalValue(value.descriptionAr),
      descriptionEn: this.optionalValue(value.descriptionEn),
      price: value.price,
      qty: value.qty,
      categoryId: value.categoryId,
      brandId: value.brandId,
      primaryImageUrl: this.optionalValue(value.primaryImageUrl),
      thumb1Url: this.optionalValue(value.thumb1Url),
      thumb2Url: this.optionalValue(value.thumb2Url),
      thumb3Url: this.optionalValue(value.thumb3Url),
      thumb4Url: this.optionalValue(value.thumb4Url),
    };
  }

  private formFromProduct(product: AdminProductDetails) {
    return {
      nameAr: product.nameAr,
      nameEn: product.nameEn,
      descriptionAr: product.descriptionAr ?? '',
      descriptionEn: product.descriptionEn ?? '',
      price: product.price,
      qty: product.qty,
      categoryId: product.category.id,
      brandId: product.brand.id,
      primaryImageUrl: product.primaryImageUrl ?? '',
      thumb1Url: product.thumb1Url ?? '',
      thumb2Url: product.thumb2Url ?? '',
      thumb3Url: product.thumb3Url ?? '',
      thumb4Url: product.thumb4Url ?? '',
    };
  }

  private emptyForm() {
    return {
      nameAr: '',
      nameEn: '',
      descriptionAr: '',
      descriptionEn: '',
      price: 0,
      qty: 0,
      categoryId: 0,
      brandId: 0,
      primaryImageUrl: '',
      thumb1Url: '',
      thumb2Url: '',
      thumb3Url: '',
      thumb4Url: '',
    };
  }

  private optionalValue(value: string): string | null {
    const normalized = value.trim();
    return normalized || null;
  }

  private resetErrors(): void {
    this.formError.set('');
    this.serverErrors.set({});
  }
}
