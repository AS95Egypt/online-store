import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, ElementRef, OnInit, ViewChild, inject } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidatorFn, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.scss',
})
export class ResetPasswordComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  @ViewChild('errorDialog') private errorDialog!: ElementRef<HTMLDialogElement>;

  private readonly passwordMatchValidator: ValidatorFn = (control: AbstractControl) => {
    const newPassword = control.get('newPassword')?.value;
    const confirmPassword = control.get('confirmPassword')?.value;

    return newPassword === confirmPassword ? null : { passwordMismatch: true };
  };

  readonly form = this.fb.nonNullable.group(
    {
      newPassword: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]],
    },
    {
      validators: this.passwordMatchValidator,
    },
  );

  email = '';
  otp = '';
  serverError = '';

  ngOnInit(): void {
    this.email = this.route.snapshot.queryParamMap.get('email') ?? '';
    this.otp = this.route.snapshot.queryParamMap.get('otp') ?? '';
  }

  onSubmit(): void {
    if (!this.email || !this.otp) {
      this.showError('This reset link is incomplete. Please request a new code.');
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.authService
      .resetPassword({
        email: this.email,
        otp: this.otp,
        newPassword: this.form.value.newPassword ?? '',
        confirmPassword: this.form.value.confirmPassword ?? '',
      })
      .subscribe({
        next: () => this.router.navigateByUrl('/login'),
        error: (error: HttpErrorResponse) => {
          const backendMessage =
            error?.error?.message ?? error?.error?.detail ?? error?.error?.title ?? error?.message;
            console.log('Backend error message:', backendMessage);
          this.serverError =
            typeof backendMessage === 'string' && backendMessage.trim()
              ? backendMessage
              : 'Unable to log in right now.';
          this.errorDialog.nativeElement.showModal();
        },
      });
  }

  closeErrorDialog(): void {
    this.errorDialog.nativeElement.close();
  }

  private showError(message: string): void {
    this.serverError = message;
    this.errorDialog.nativeElement.showModal();
  }

  private getBackendErrorMessage(error: HttpErrorResponse): string {
    const body = error.error as {
      message?: unknown;
      detail?: unknown;
      title?: unknown;
      errors?: Record<string, unknown>;
    } | null;

    for (const candidate of [body?.message, body?.detail]) {
      if (typeof candidate === 'string' && candidate.trim()) {
        return candidate;
      }
    }

    if (body?.errors) {
      const validationMessages = Object.values(body.errors)
        .flatMap((value) => (Array.isArray(value) ? value : [value]))
        .filter((value): value is string => typeof value === 'string' && Boolean(value.trim()));
      if (validationMessages.length > 0) {
        return validationMessages.join(' ');
      }
    }

    return typeof body?.message === 'string' && body.message.trim()
      ? body.message
      : 'Unable to reset the password.';
  }
}
