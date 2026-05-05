import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonModule } from '@angular/material/button';
import { GearService } from '../../services/gear.service';
import { HttpErrorResponse } from '@angular/common/http';

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
export class GearItemDialogComponent implements OnInit {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<GearItemDialogComponent>);
  private gearService = inject(GearService);
  public data = inject(MAT_DIALOG_DATA);

  serverErrors: any = {};

  gearForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    weightInGrams: [0, [Validators.required, Validators.min(0)]],
    quantity: [1, [Validators.required, Validators.min(1)]],
    isGroupGear: [false],
    isWearable: [false],
    categoryId: [null, Validators.required]
  });

  ngOnInit(): void {
    if (this.data) {
      if (this.data.item) {
        this.gearForm.patchValue(this.data.item);
      }
      if (this.data.categoryId) {
        this.gearForm.patchValue({ categoryId: this.data.categoryId });
      }
    }
  }

  close(): void {
    this.dialogRef.close();
  }

  save(): void {
    if (this.gearForm.valid) {
      this.serverErrors = {};
      const gearData = this.gearForm.value;
      const isEdit = !!this.data.item;

      const request = isEdit
        ? this.gearService.updateGearItem(this.data.item.id, gearData)
        : this.gearService.addCustomGearItem(gearData.categoryId, gearData);

      request.subscribe({
        next: () => {
          this.dialogRef.close(true);
        },
        error: (err: HttpErrorResponse) => {
          if (err.status === 400 && err.error?.errors) {
            this.serverErrors = err.error.errors;
            Object.keys(this.serverErrors).forEach(key => {
              const controlName = key.charAt(0).toLowerCase() + key.slice(1);
              const control = this.gearForm.get(controlName);
              if (control) {
                control.setErrors({ serverError: this.serverErrors[key][0] });
              }
            });
          }
          console.error('Помилка збереження речі:', err);
        }
      });
    }
  }
}
