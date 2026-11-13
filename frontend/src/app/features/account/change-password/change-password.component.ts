import { HttpErrorResponse } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidatorFn, Validators } from '@angular/forms';
import { AccountService } from '../../../core/account/account.service';

// Keep in sync with backend/src/OnlineStore.Application/Auth/PasswordPolicy.cs.
const minimumPasswordLength = 8;

const passwordStrengthValidator: ValidatorFn = (control: AbstractControl) => {
  const password = typeof control.value === 'string' ? control.value : '';
  return /\p{Lu}/u.test(password) && /\p{Ll}/u.test(password) && /\p{Nd}/u.test(password)
    ? null
    : { passwordStrength: true };
};

const passwordMatchValidator: ValidatorFn = (control: AbstractControl) =>
  control.get('newPassword')?.value === control.get('confirmPassword')?.value
    ? null
    : { passwordMismatch: true };

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './change-password.component.html',
  styleUrl: './change-password.component.scss',
})
export class ChangePasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly accountService = inject(AccountService);

  readonly form = this.fb.nonNullable.group(
    {
      currentPassword: ['', Validators.required],
      newPassword: [
        '',
        [
          Validators.required,
          Validators.minLength(minimumPasswordLength),
          passwordStrengthValidator,
        ],
      ],
      confirmPassword: ['', Validators.required],
    },
    { validators: passwordMatchValidator },
  );

  isSaving = false;
  currentPasswordServerError = '';
  newPasswordServerError = '';
  formError = '';
  successMessage = '';

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.currentPasswordServerError = '';
    this.newPasswordServerError = '';
    this.formError = '';
    this.successMessage = '';
    this.isSaving = true;

    this.accountService.changePassword(this.form.getRawValue()).subscribe({
      next: () => {
        this.form.reset();
        this.successMessage = 'Your password has been changed.';
        this.isSaving = false;
      },
      error: (error: HttpErrorResponse) => {
        const body = error.error as {
          message?: unknown;
          detail?: unknown;
          errors?: Record<string, unknown>;
        } | null;

        this.currentPasswordServerError = this.getFieldError(body?.errors, 'CurrentPassword');
        this.newPasswordServerError = this.getFieldError(body?.errors, 'NewPassword');
        const message = this.getMessage(body);
        if (!this.currentPasswordServerError && /current password is incorrect/i.test(message)) {
          this.currentPasswordServerError = message;
        } else if (!this.newPasswordServerError && message) {
          this.formError = message;
        }

        if (!this.currentPasswordServerError && !this.newPasswordServerError && !this.formError) {
          this.formError = 'Unable to change your password.';
        }
        this.isSaving = false;
      },
    });
  }

  private getFieldError(errors: Record<string, unknown> | undefined, field: string): string {
    const value = errors?.[field] ?? errors?.[field.toLowerCase()];
    if (Array.isArray(value)) {
      return value.find((item): item is string => typeof item === 'string') ?? '';
    }

    return typeof value === 'string' ? value : '';
  }

  private getMessage(body: {
    message?: unknown;
    detail?: unknown;
  } | null): string {
    for (const value of [body?.message, body?.detail]) {
      if (typeof value === 'string' && value.trim()) {
        return value;
      }
    }

    return '';
  }
}
