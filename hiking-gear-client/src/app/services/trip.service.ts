import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Trip {
  id: number;
  userId: number;
  title: string;
  locationName: string;
  latitude: number;
  longitude: number;
  startDate: string;
  endDate: string;
  groupSize: number;
  accommodationFormat: number;
}

@Injectable({
  providedIn: 'root'
})
export class TripService {
  private readonly apiUrl = 'https://localhost:7285/api/Trip';
  private readonly http = inject(HttpClient);

  getAll(): Observable<Trip[]> {
    return this.http.get<Trip[]>(this.apiUrl);
  }
}
