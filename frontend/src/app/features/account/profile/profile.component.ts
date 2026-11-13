import { HttpErrorResponse } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidatorFn, Validators } from '@angular/forms';
import { AccountService } from '../../../core/account/account.service';
import { CustomerProfile } from '../../../core/account/account.models';

const trimmedNameValidator: ValidatorFn = (control: AbstractControl) =>
  typeof control.value === 'string' && control.value.trim().length > 0
    ? null
    : { required: true };

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss',
})
export class ProfileComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly accountService = inject(AccountService);

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200), trimmedNameValidator]],
  });

  profile = signal<CustomerProfile | null>(null);
  isLoading = signal(true);
  isSaving = signal(false);
  loadError = signal('');
  nameServerError = signal('');
  formError = signal('');
  successMessage = signal('');

  ngOnInit(): void {
    this.accountService.getProfile().subscribe({
      next: (profile) => {
        this.profile.set(profile);
        this.form.controls.name.setValue(profile.name);
        this.isLoading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.loadError.set(this.getMessage(error, 'Unable to load your profile.'));
        this.isLoading.set(false);
      },
    });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.nameServerError.set('');
    this.formError.set('');
    this.successMessage.set('');
    this.isSaving.set(true);

    this.accountService.updateProfile({ name: this.form.controls.name.value.trim() }).subscribe({
      next: (profile) => {
        this.profile.set(profile);
        this.form.controls.name.setValue(profile.name);
        this.successMessage.set('Your profile has been updated.');
        this.isSaving.set(false);
      },
      error: (error: HttpErrorResponse) => {
        const nameError = this.getFieldError(error, 'Name');
        this.nameServerError.set(nameError);
        if (!nameError) {
          this.formError.set(this.getMessage(error, 'Unable to update your profile.'));
        }
        this.isSaving.set(false);
      },
    });
  }

  private getFieldError(error: HttpErrorResponse, field: string): string {
    const body = error.error as { errors?: Record<string, unknown> } | null;
    const errors = body?.errors;
    const fieldErrors = errors?.[field] ?? errors?.[field.toLowerCase()];
    if (Array.isArray(fieldErrors)) {
      return fieldErrors.find((value): value is string => typeof value === 'string') ?? '';
    }

    return typeof fieldErrors === 'string' ? fieldErrors : '';
  }

  private getMessage(error: HttpErrorResponse, fallback: string): string {
    const body = error.error as { message?: unknown; detail?: unknown; title?: unknown } | null;
    for (const candidate of [body?.message, body?.detail, body?.title]) {
      if (typeof candidate === 'string' && candidate.trim()) {
        return candidate;
      }
    }

    return fallback;
  }
}
