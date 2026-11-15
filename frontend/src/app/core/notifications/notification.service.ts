import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  readonly message = signal('');
  private timeoutId: ReturnType<typeof setTimeout> | null = null;

  show(message: string): void {
    if (this.timeoutId !== null) {
      clearTimeout(this.timeoutId);
    }

    this.message.set(message);
    this.timeoutId = setTimeout(() => this.dismiss(), 3500);
  }

  dismiss(): void {
    if (this.timeoutId !== null) {
      clearTimeout(this.timeoutId);
      this.timeoutId = null;
    }

    this.message.set('');
  }
}
