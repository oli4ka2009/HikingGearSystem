import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';

export interface ConfirmDialogData {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
}

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>{{ data.title }}</h2>
    <mat-dialog-content>
      <p>{{ data.message }}</p>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onDismiss()">{{ data.cancelText || 'Скасувати' }}</button>
      <button mat-flat-button color="primary" (click)="onConfirm()">{{ data.confirmText || 'Підтвердити' }}</button>
    </mat-dialog-actions>
  `,
  styles: [`
    mat-dialog-content {
      min-width: 300px;
    }
    mat-dialog-actions {
      padding: 16px 24px !important;
    }
    button[color="primary"] {
      border-radius: 20px !important;
      padding: 0 24px !important;
    }
    button[mat-button]:not([color="primary"]), 
    button[mat-stroked-button] {
      border-radius: 20px !important;
      overflow: hidden !important;
      padding: 0 24px !important;
    }
  `]
})
export class ConfirmDialogComponent {
  private dialogRef = inject(MatDialogRef<ConfirmDialogComponent>);
  public data: ConfirmDialogData = inject(MAT_DIALOG_DATA);

  onDismiss(): void {
    this.dialogRef.close(false);
  }

  onConfirm(): void {
    this.dialogRef.close(true);
  }
}
