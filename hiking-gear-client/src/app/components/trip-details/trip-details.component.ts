import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { forkJoin } from 'rxjs';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatListModule } from '@angular/material/list';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { GearService, TripGearResponseDto, GearItem } from '../../services/gear.service';

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
    MatProgressBarModule
  ],
  templateUrl: './trip-details.component.html',
  styleUrl: './trip-details.component.css'
})
export class TripDetailsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly gearService = inject(GearService);

  readonly tripId = this.route.snapshot.paramMap.get('id');
  isGenerating = signal<boolean>(false);
  gearList = signal<TripGearResponseDto | null>(null);

  // Advanced Packing Statistics (Calculated from list to keep in sync)
  packingProgress = computed<PackingProgressDto | null>(() => {
    const list = this.gearList();
    if (!list || !list.categories) return {
      totalItems: 0, packedItems: 0, progressPercentage: 0,
      individualPackWeightGrams: 0, individualWornWeightGrams: 0, groupTotalWeightGrams: 0,
      individualProgressPercentage: 0, groupProgressPercentage: 0
    };

    const allItems = list.categories.flatMap(c => c.items || []);
    if (allItems.length === 0) {
      return {
        totalItems: 0, packedItems: 0, progressPercentage: 0,
        individualPackWeightGrams: 0, individualWornWeightGrams: 0, groupTotalWeightGrams: 0,
        individualProgressPercentage: 0, groupProgressPercentage: 0
      };
    }

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
      } else {
        console.error('ТЕРМІНОВО: ID походу не знайдено у параметрах маршруту!');
      }
    });
  }

  private loadTripData(id: number): void {
    forkJoin({
      gear: this.gearService.getGearList(id),
      progress: this.gearService.getPackingProgress(id)
    }).subscribe({
      next: (data) => {
        this.gearList.set(data.gear);
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

  addItem(categoryName: string): void {
    console.log('Додати річ у категорію:', categoryName);
  }
}
