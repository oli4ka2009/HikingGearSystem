import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-gear-item-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatCheckboxModule,
    MatButtonModule,
  ],
  templateUrl: './gear-item-dialog.component.html',
  styleUrl: './gear-item-dialog.component.css',
})
export class GearItemDialogComponent {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<GearItemDialogComponent>);
  public data = inject(MAT_DIALOG_DATA);

  gearForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    weightInGrams: [0, [Validators.required, Validators.min(0)]],
    quantity: [1, [Validators.required, Validators.min(1)]],
    isGroupGear: [false],
    isWearable: [false],
  });

  close(): void {
    this.dialogRef.close();
  }

  save(): void {
    if (this.gearForm.valid) {
      this.dialogRef.close(this.gearForm.value);
    }
  }
}
