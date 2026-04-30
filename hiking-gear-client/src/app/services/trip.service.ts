import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export enum AccommodationFormat { None = 0, Tent = 1, Shelter = 2 }

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
  accommodationFormat: AccommodationFormat;
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

  getById(id: number): Observable<Trip> {
    return this.http.get<Trip>(`${this.apiUrl}/${id}`);
  }

  createTrip(tripData: any): Observable<any> {
    return this.http.post<any>(this.apiUrl, tripData);
  }

  updateTrip(id: number, data: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, data);
  }

  deleteTrip(tripId: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${tripId}`);
  }
}
