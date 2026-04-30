import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatListModule } from '@angular/material/list';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { GearService, TripGearResponseDto, GearItem } from '../../services/gear.service';
import { AccommodationFormat, Trip, TripService } from '../../services/trip.service';

export interface PackingProgressDto {
  totalItems: number;
  packedItems: number;
  progressPercentage: number;
  individualPackWeightGrams: number;
  individualWornWeightGrams: number;
  groupTotalWeightGrams: number;
  individualProgressPercentage: number;
  groupProgressPercentage: number;
}

import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { GearItemDialogComponent } from '../gear-item-dialog/gear-item-dialog.component';
import { CategoryService } from '../../services/category.service';
import { CategoryDialogComponent } from '../category-dialog/category-dialog.component';

@Component({
  selector: 'app-trip-details',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatListModule,
    MatCardModule,
    MatCheckboxModule,
    MatProgressBarModule,
    MatDialogModule,
    MatSnackBarModule
  ],
  templateUrl: './trip-details.component.html',
  styleUrl: './trip-details.component.css'
})
export class TripDetailsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly gearService = inject(GearService);
  private readonly tripService = inject(TripService);
  private readonly categoryService = inject(CategoryService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly tripId = this.route.snapshot.paramMap.get('id');
  readonly AccommodationFormat = AccommodationFormat;
  isGenerating = signal<boolean>(false);
  gearList = signal<TripGearResponseDto | null>(null);
  trip = signal<Trip | null>(null);

  // Advanced Packing Statistics (Calculated from list to keep in sync)
  packingProgress = computed<PackingProgressDto | null>(() => {
    const list = this.gearList();
    // Повертаємо null якщо даних нема — щоб @if в шаблоні приховував блок
    if (!list || !list.categories) return null;

    const allItems = list.categories.flatMap(c => c.items || []);
    if (allItems.length === 0) return null;

    const individualItems = allItems.filter(i => !i.isGroupGear);
    const groupItems = allItems.filter(i => i.isGroupGear);

    const calcProgress = (items: GearItem[]) =>
      items.length > 0 ? Math.round((items.filter(i => i.isPacked).length / items.length) * 100) : 0;

    return {
      totalItems: allItems.length,
      packedItems: allItems.filter(i => i.isPacked).length,
      progressPercentage: calcProgress(allItems),

      individualPackWeightGrams: individualItems
        .filter(i => !i.isWearable)
        .reduce((acc, i) => acc + (i.weightInGrams * i.quantity), 0),

      individualWornWeightGrams: individualItems
        .filter(i => i.isWearable)
        .reduce((acc, i) => acc + (i.weightInGrams * i.quantity), 0),

      groupTotalWeightGrams: groupItems
        .reduce((acc, i) => acc + (i.weightInGrams * i.quantity), 0),

      individualProgressPercentage: calcProgress(individualItems),
      groupProgressPercentage: calcProgress(groupItems)
    };
  });

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        const numericId = Number(id);
        this.loadTripData(numericId);
        this.loadTripDetails(numericId);
      } else {
        console.error('ТЕРМІНОВО: ID походу не знайдено у параметрах маршруту!');
      }
    });
  }

  private loadTripDetails(id: number): void {
    this.tripService.getById(id).subscribe({
      next: (data) => this.trip.set(data),
      error: (err) => console.error('Помилка завантаження деталей походу:', err)
    });
  }

  private loadTripData(id: number): void {
    this.gearService.getGearList(id).subscribe({
      next: (data) => {
        this.gearList.set(data);
        console.log(`Успішно завантажено дані для походу #${id}`);
      },
      error: (err) => {
        console.error(`КРИТИЧНА ПОМИЛКА: Не вдалося завантажити дані для походу #${id}:`, err);
      }
    });
  }

  generateGear(): void {
    if (!this.tripId) return;

    this.isGenerating.set(true);
    this.gearService.generateGearForTrip(Number(this.tripId)).subscribe({
      next: (response) => {
        this.isGenerating.set(false);
        this.gearList.set(response);
      },
      error: (err) => {
        this.isGenerating.set(false);
        console.error('Помилка генерації:', err);
      }
    });
  }

  togglePacked(item: GearItem): void {
    const originalStatus = item.isPacked;
    const newStatus = !originalStatus;

    // Optimistic update
    item.isPacked = newStatus;
    this.gearList.set({ ...this.gearList()! });

    this.gearService.togglePacked(item.id, newStatus).subscribe({
      error: (err) => {
        item.isPacked = originalStatus; // Rollback
        this.gearList.set({ ...this.gearList()! });
        console.error('Помилка оновлення статусу:', err);
      }
    });
  }

  deleteItem(itemId: number): void {
    if (confirm('Ви впевнені, що хочете видалити цю річ?')) {
      this.gearService.deleteGearItem(itemId).subscribe({
        next: () => {
          const currentList = this.gearList();
          if (currentList) {
            const updatedCategories = currentList.categories.map(category => ({
              ...category,
              items: category.items.filter(item => item.id !== itemId)
            }));
            this.gearList.set({ categories: updatedCategories });
          }
        },
        error: (err) => console.error('Помилка при видаленні:', err)
      });
    }
  }

  openAddItemDialog(categoryName: string): void {
    const dialogRef = this.dialog.open(GearItemDialogComponent, {
      width: '400px',
      data: { categoryName }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result && this.tripId) {
        this.gearService.addCustomGearItem(Number(this.tripId), categoryName, result).subscribe({
          next: () => {
            this.snackBar.open('Річ успішно додано', 'Закрити', { duration: 3000 });
            this.loadTripData(Number(this.tripId));
          },
          error: (err) => {
            console.error('Помилка при додаванні речі:', err);
            this.snackBar.open('Помилка при додаванні речі', 'Закрити', { duration: 3000 });
          }
        });
      }
    });
  }

  openCategoryDialog(): void {
    const dialogRef = this.dialog.open(CategoryDialogComponent, {
      width: '400px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.categoryService.createCategory(result).subscribe({
          next: () => {
            this.snackBar.open('Категорію успішно додано', 'Закрити', { duration: 3000 });
            if (this.tripId) {
              this.loadTripData(Number(this.tripId));
            }
          },
          error: (err) => {
            console.error('Помилка при додаванні категорії:', err);
            this.snackBar.open('Помилка при додаванні категорії', 'Закрити', { duration: 3000 });
          }
        });
      }
    });
  }
}
