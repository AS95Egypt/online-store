import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { readAdminApiError } from '../../../core/admin/admin-api-errors';
import { AdminBrand, AdminBrandRequest } from '../../../core/brands/admin-brands.models';
import { AdminBrandsService } from '../../../core/brands/admin-brands.service';
import { NotificationService } from '../../../core/notifications/notification.service';

@Component({
  selector: 'app-admin-brands',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './admin-brands.component.html',
  styleUrl: '../admin-catalog.scss',
})
export class AdminBrandsComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly brandsService = inject(AdminBrandsService);
  private readonly notificationService = inject(NotificationService);

  readonly brands = signal<AdminBrand[]>([]);
  readonly isLoading = signal(true);
  readonly loadError = signal('');
  readonly showForm = signal(false);
  readonly editingId = signal<number | null>(null);
  readonly isSaving = signal(false);
  readonly formError = signal('');
  readonly serverErrors = signal<Record<string, string>>({});
  readonly form = this.fb.nonNullable.group({
    nameAr: ['', [Validators.required, Validators.maxLength(200)]],
    nameEn: ['', [Validators.required, Validators.maxLength(200)]],
    displayImageUrl: [''],
  });

  ngOnInit(): void {
    this.loadBrands();
  }

  loadBrands(): void {
    this.isLoading.set(true);
    this.loadError.set('');
    this.brandsService.list().subscribe({
      next: (response) => {
        this.brands.set(response.data);
        this.isLoading.set(false);
      },
      error: () => {
        this.loadError.set('Brands could not be loaded. Please try again.');
        this.isLoading.set(false);
      },
    });
  }

  startCreate(): void {
    this.editingId.set(null);
    this.form.reset({ nameAr: '', nameEn: '', displayImageUrl: '' });
    this.resetErrors();
    this.showForm.set(true);
  }

  edit(brand: AdminBrand): void {
    this.editingId.set(brand.id);
    this.form.reset({
      nameAr: brand.nameAr,
      nameEn: brand.nameEn,
      displayImageUrl: brand.displayImageUrl ?? '',
    });
    this.resetErrors();
    this.showForm.set(true);
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
    const request: AdminBrandRequest = {
      nameAr: this.form.controls.nameAr.value.trim(),
      nameEn: this.form.controls.nameEn.value.trim(),
      displayImageUrl: this.form.controls.displayImageUrl.value.trim() || null,
    };
    const editingId = this.editingId();
    const saveRequest = editingId === null
      ? this.brandsService.create(request)
      : this.brandsService.update(editingId, request);
    saveRequest.subscribe({
      next: (response) => {
        this.brands.update((items) => {
          const updated = editingId === null
            ? [...items, response.data]
            : items.map((item) => item.id === editingId ? response.data : item);
          return updated.sort((a, b) => a.nameEn.localeCompare(b.nameEn));
        });
        this.showForm.set(false);
        this.isSaving.set(false);
        this.notificationService.show(editingId === null ? 'Brand created.' : 'Brand updated.');
      },
      error: (error: unknown) => {
        const parsed = readAdminApiError(error, 'Unable to save this brand.');
        this.formError.set(parsed.message);
        this.serverErrors.set(parsed.fields);
        this.isSaving.set(false);
      },
    });
  }

  private resetErrors(): void {
    this.formError.set('');
    this.serverErrors.set({});
  }
}
