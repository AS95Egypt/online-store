import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { NotificationService } from '../../../core/notifications/notification.service';
import { readAdminApiError } from '../../../core/admin/admin-api-errors';
import { AdminCategoriesService } from '../../../core/categories/admin-categories.service';
import { AdminCategory, AdminCategoryRequest } from '../../../core/categories/admin-categories.models';

@Component({
  selector: 'app-admin-categories',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './admin-categories.component.html',
  styleUrl: '../admin-catalog.scss',
})
export class AdminCategoriesComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly categoriesService = inject(AdminCategoriesService);
  private readonly notificationService = inject(NotificationService);

  readonly categories = signal<AdminCategory[]>([]);
  readonly isLoading = signal(true);
  readonly loadError = signal('');
  readonly showForm = signal(false);
  readonly editingId = signal<number | null>(null);
  readonly isSaving = signal(false);
  readonly changingStatusId = signal<number | null>(null);
  readonly formError = signal('');
  readonly serverErrors = signal<Record<string, string>>({});
  readonly form = this.fb.nonNullable.group({
    nameAr: ['', [Validators.required, Validators.maxLength(200)]],
    nameEn: ['', [Validators.required, Validators.maxLength(200)]],
    displayImageUrl: [''],
  });

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.isLoading.set(true);
    this.loadError.set('');
    this.categoriesService.list().subscribe({
      next: (response) => {
        this.categories.set(response.data);
        this.isLoading.set(false);
      },
      error: () => {
        this.loadError.set('Categories could not be loaded. Please try again.');
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

  edit(category: AdminCategory): void {
    this.editingId.set(category.id);
    this.form.reset({
      nameAr: category.nameAr,
      nameEn: category.nameEn,
      displayImageUrl: category.displayImageUrl ?? '',
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
    const request: AdminCategoryRequest = {
      nameAr: this.form.controls.nameAr.value.trim(),
      nameEn: this.form.controls.nameEn.value.trim(),
      displayImageUrl: this.optionalValue(this.form.controls.displayImageUrl.value),
    };
    const editingId = this.editingId();
    const saveRequest = editingId === null
      ? this.categoriesService.create(request)
      : this.categoriesService.update(editingId, request);
    saveRequest.subscribe({
      next: (response) => {
        this.categories.update((items) => {
          const updated = editingId === null
            ? [...items, response.data]
            : items.map((item) => item.id === editingId ? response.data : item);
          return updated.sort((a, b) => a.nameEn.localeCompare(b.nameEn));
        });
        this.showForm.set(false);
        this.isSaving.set(false);
        this.notificationService.show(editingId === null ? 'Category created.' : 'Category updated.');
      },
      error: (error: unknown) => this.handleSaveError(error),
    });
  }

  toggleStatus(category: AdminCategory): void {
    const action = category.isActive ? 'deactivate' : 'activate';
    if (!window.confirm(`Are you sure you want to ${action} ${category.nameEn}?`)) {
      return;
    }

    this.changingStatusId.set(category.id);
    this.categoriesService.setStatus(category.id, !category.isActive).subscribe({
      next: (response) => {
        this.categories.update((items) =>
          items.map((item) => item.id === category.id ? response.data : item),
        );
        this.changingStatusId.set(null);
        this.notificationService.show(`Category ${action}d.`);
      },
      error: (error: unknown) => {
        this.changingStatusId.set(null);
        this.notificationService.show(readAdminApiError(error, 'Unable to update category status.').message);
      },
    });
  }

  private handleSaveError(error: unknown): void {
    const parsed = readAdminApiError(error, 'Unable to save this category.');
    this.formError.set(parsed.message);
    this.serverErrors.set(parsed.fields);
    this.isSaving.set(false);
  }

  private resetErrors(): void {
    this.formError.set('');
    this.serverErrors.set({});
  }

  private optionalValue(value: string): string | null {
    const normalized = value.trim();
    return normalized || null;
  }
}
