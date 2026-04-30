import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

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
  id: number;
  categoryName: string;
  items: GearItem[];
}

export interface TripGearResponseDto {
  categories: GearCategory[];
}

@Injectable({
  providedIn: 'root'
})
export class GearService {
  private readonly apiGenerationUrl = 'https://localhost:7285/api/Gear';
  private readonly apiGearItemUrl = 'https://localhost:7285/api/GearItem';
  private readonly http = inject(HttpClient);

  getGearList(tripId: number): Observable<TripGearResponseDto> {
    return this.http.get<TripGearResponseDto>(`${this.apiGearItemUrl}/trip/${tripId}`);
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

  addCustomGearItem(categoryId: number, itemData: any): Observable<void> {
    const payload = {
      ...itemData,
      categoryId
    };
    return this.http.post<void>(this.apiGearItemUrl, payload);
  }

  updateGearItem(id: number, data: any): Observable<void> {
    return this.http.put<void>(`${this.apiGearItemUrl}/${id}`, data);
  }
}
