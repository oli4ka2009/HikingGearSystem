import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface GearItem {
  name: string;
  quantity: number;
  weightInGrams: number;
  isGroupGear: boolean;
  isWearable: boolean;
}

export interface GearCategory {
  categoryName: string;
  items: GearItem[];
}

export interface AiGearResponseDto {
  categories: GearCategory[];
}

@Injectable({
  providedIn: 'root'
})
export class GearService {
  private readonly apiUrl = 'https://localhost:7285/api/Gear';
  private readonly http = inject(HttpClient);

  generateGearForTrip(tripId: number): Observable<AiGearResponseDto> {
    return this.http.post<AiGearResponseDto>(`${this.apiUrl}/generate/${tripId}`, {});
  }
}
