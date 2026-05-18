import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatListModule } from '@angular/material/list';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import { GearService, TripGearResponseDto, GearItem } from '../../services/gear.service';
import { AccommodationFormat, Trip, TripService } from '../../services/trip.service';
import { AuthService } from '../../services/auth.service';
import { CategoryService } from '../../services/category.service';

import { GearItemDialogComponent } from '../gear-item-dialog/gear-item-dialog.component';
import { CategoryDialogComponent } from '../category-dialog/category-dialog.component';
import { TripEditDialogComponent } from '../trip-edit-dialog/trip-edit-dialog.component';
import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog.component';
import { LoadingDialogComponent } from '../loading-dialog/loading-dialog.component';

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
    MatSnackBarModule,
    MatTooltipModule,
    MatFormFieldModule,
    MatInputModule,
    ConfirmDialogComponent
  ],
  templateUrl: './trip-details.component.html',
  styleUrl: './trip-details.component.css'
})
export class TripDetailsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);
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
  searchQuery = signal<string>('');

  // Computed filtered list
  filteredCategories = computed(() => {
    const list = this.gearList();
    const query = this.searchQuery().toLowerCase().trim();
    
    if (!list || !list.categories) return [];
    if (!query) return list.categories;

    return list.categories
      .map(category => ({
        ...category,
        items: category.items.filter(item => 
          item.name.toLowerCase().includes(query)
        )
      }))
      .filter(category => category.items.length > 0);
  });

  // Advanced Packing Statistics (Calculated from list to keep in sync)
  packingProgress = computed<PackingProgressDto | null>(() => {
    const list = this.gearList();
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

    if (this.gearList() && this.gearList()!.categories.length > 0) {
      const dialogRef = this.dialog.open(ConfirmDialogComponent, {
        width: '350px',
        data: {
          title: 'Перегенерувати список?',
          message: 'Ви впевнені? Це видалить всі поточні речі в цьому поході і створить новий список.',
          confirmText: 'Перегенерувати'
        }
      });

      dialogRef.afterClosed().subscribe(result => {
        if (result) {
          this.executeGeneration();
        }
      });
    } else {
      this.executeGeneration();
    }
  }

  private executeGeneration(): void {
    const loadingRef = this.dialog.open(LoadingDialogComponent, { 
      disableClose: true,
      width: '300px'
    });

    this.isGenerating.set(true);
    this.gearService.generateGearForTrip(Number(this.tripId)).subscribe({
      next: (response) => {
        loadingRef.close();
        this.isGenerating.set(false);
        this.gearList.set(response);
        this.snackBar.open('Список успішно згенеровано', 'Закрити', { duration: 3000 });
      },
      error: (err) => {
        loadingRef.close();
        this.isGenerating.set(false);
        console.error('Помилка генерації:', err);
        this.snackBar.open('Помилка при генерації списку', 'Закрити', { duration: 3000 });
      }
    });
  }

  deleteCategory(category: any): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '350px',
      data: {
        title: 'Видалити категорію?',
        message: `Ви впевнені, що хочете видалити категорію '${category.categoryName}' та всі речі в ній?`
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.categoryService.deleteCategory(category.id).subscribe({
          next: () => {
            this.snackBar.open('Категорію видалено', 'Закрити', { duration: 3000 });
            if (this.tripId) this.loadTripData(Number(this.tripId));
          },
          error: (err) => console.error('Помилка при видаленні категорії:', err)
        });
      }
    });
  }

  togglePacked(item: GearItem): void {
    const originalStatus = item.isPacked;
    const newStatus = !originalStatus;

    item.isPacked = newStatus;
    this.gearList.set({ ...this.gearList()! });

    this.gearService.togglePacked(item.id, newStatus).subscribe({
      error: (err) => {
        item.isPacked = originalStatus;
        this.gearList.set({ ...this.gearList()! });
        console.error('Помилка оновлення статусу:', err);
      }
    });
  }

  deleteItem(item: GearItem): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '350px',
      data: {
        title: 'Видалити річ?',
        message: `Ви впевнені, що хочете видалити '${item.name}'?`
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.gearService.deleteGearItem(item.id).subscribe({
          next: () => {
            const currentList = this.gearList();
            if (currentList) {
              const updatedCategories = currentList.categories.map(category => ({
                ...category,
                items: category.items.filter(i => i.id !== item.id)
              }));
              this.gearList.set({ categories: updatedCategories });
              this.snackBar.open('Річ видалено', 'Закрити', { duration: 3000 });
            }
          },
          error: (err) => console.error('Помилка при видаленні:', err)
        });
      }
    });
  }

  openAddItemDialog(category: { id: number; categoryName: string }): void {
    const dialogRef = this.dialog.open(GearItemDialogComponent, {
      width: '400px',
      data: { categoryId: category.id, categoryName: category.categoryName }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result === true && this.tripId) {
        this.loadTripData(Number(this.tripId));
      }
    });
  }

  openCategoryDialog(): void {
    const dialogRef = this.dialog.open(CategoryDialogComponent, {
      width: '400px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.categoryService.createCategory({ name: result.name, tripId: Number(this.tripId) }).subscribe({
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

  openEditTripDialog(): void {
    const currentTrip = this.trip();
    if (!currentTrip) return;

    const dialogRef = this.dialog.open(TripEditDialogComponent, {
      width: '500px',
      data: { trip: currentTrip, tripId: currentTrip.id }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result === true) {
        this.loadTripDetails(currentTrip.id);
        this.snackBar.open('Деталі походу оновлено', 'Закрити', { duration: 3000 });
      }
    });
  }

  openEditCategoryDialog(category: any): void {
    const dialogRef = this.dialog.open(CategoryDialogComponent, {
      width: '400px',
      data: { id: category.id, name: category.categoryName }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.categoryService.updateCategory(category.id, result).subscribe({
          next: () => {
            this.snackBar.open('Категорію оновлено', 'Закрити', { duration: 3000 });
            if (this.tripId) this.loadTripData(Number(this.tripId));
          },
          error: (err) => console.error('Помилка оновлення категорії:', err)
        });
      }
    });
  }

  openEditItemDialog(item: GearItem, categoryId: number): void {
    const dialogRef = this.dialog.open(GearItemDialogComponent, {
      width: '400px',
      data: { item, categoryId }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result === true && this.tripId) {
        this.loadTripData(Number(this.tripId));
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
