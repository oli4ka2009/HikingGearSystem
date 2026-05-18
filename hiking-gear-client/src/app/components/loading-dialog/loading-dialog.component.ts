import { Component } from '@angular/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialogModule } from '@angular/material/dialog';

@Component({
  selector: 'app-loading-dialog',
  standalone: true,
  imports: [MatDialogModule, MatProgressSpinnerModule],
  template: `
    <div style="padding: 24px; display: flex; flex-direction: column; align-items: center; gap: 16px;">
      <mat-spinner diameter="50"></mat-spinner>
      <p style="margin: 0; font-family: 'Inter', sans-serif; font-weight: 500; color: #333;">
        Генеруємо список спорядження...
      </p>
    </div>
  `
})
export class LoadingDialogComponent {}
