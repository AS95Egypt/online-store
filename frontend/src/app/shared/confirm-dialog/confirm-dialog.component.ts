import { Component, EventEmitter, Input, Output } from '@angular/core';

export interface ConfirmDialogItem {
  name: string;
  quantity: number;
}

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  templateUrl: './confirm-dialog.component.html',
  styleUrl: './confirm-dialog.component.scss',
})
export class ConfirmDialogComponent {
  @Input() title = 'Confirm action';
  @Input() message = '';
  @Input() confirmLabel = 'Confirm';
  @Input() danger = false;
  @Input() busy = false;
  @Input() items: readonly ConfirmDialogItem[] = [];

  @Output() confirmed = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();
}
