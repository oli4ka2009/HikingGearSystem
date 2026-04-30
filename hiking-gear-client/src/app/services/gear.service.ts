import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';

export interface GearItem {
  id: number;
  name: string;
  quantity: number;
  weightInGrams: number;
  isGroupGear: boolean;
  isWearable: boolean;
  isPacked: boolean;
}

export interface GearCategory {
  categoryName: string;
  items: GearItem[];
}

export interface TripGearResponseDto {
  categories: GearCategory[];
}

// Тип який реально повертає API
interface ApiGearItem {
  id: number;
  name: string;
  quantity: number;
  weightInGrams: number;
  isGroupGear: boolean;
  isWearable: boolean;
  isPacked: boolean;
  category: { id: number; name: string; gearItems: any[] };
}

@Injectable({
  providedIn: 'root'
})
export class GearService {
  private readonly apiGenerationUrl = 'https://localhost:7285/api/Gear';
  private readonly apiGearItemUrl = 'https://localhost:7285/api/GearItem';
  private readonly http = inject(HttpClient);

  getGearList(tripId: number): Observable<TripGearResponseDto> {
    return this.http.get<ApiGearItem[]>(`${this.apiGearItemUrl}/trip/${tripId}`).pipe(
      map(items => {
        // Групуємо плаский масив по category.name
        const grouped = new Map<string, GearItem[]>();
        for (const item of items) {
          const catName = item.category?.name ?? 'Інше';
          if (!grouped.has(catName)) grouped.set(catName, []);
          grouped.get(catName)!.push({
            id: item.id,
            name: item.name,
            quantity: item.quantity,
            weightInGrams: item.weightInGrams,
            isGroupGear: item.isGroupGear,
            isWearable: item.isWearable,
            isPacked: item.isPacked,
          });
        }
        return {
          categories: Array.from(grouped.entries()).map(([categoryName, items]) => ({
            categoryName,
            items,
          }))
        };
      })
    );
  }

  getPackingProgress(tripId: number): Observable<any> {
    return this.http.get<any>(`${this.apiGearItemUrl}/trip/${tripId}/progress`);
  }

  generateGearForTrip(tripId: number): Observable<TripGearResponseDto> {
    return this.http.post<TripGearResponseDto>(`${this.apiGenerationUrl}/generate/${tripId}`, {});
  }

  togglePacked(gearId: number, isPacked: boolean): Observable<void> {
    return this.http.patch<void>(`${this.apiGearItemUrl}/${gearId}/pack`, { isPacked });
  }

  deleteGearItem(itemId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiGearItemUrl}/${itemId}`);
  }

  addCustomGearItem(tripId: number, categoryName: string, itemData: any): Observable<void> {
    const payload = {
      ...itemData,
      tripId,
      categoryName
    };
    return this.http.post<void>(this.apiGearItemUrl, payload);
  }
}
