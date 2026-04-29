import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TripService {
  private readonly apiUrl = 'https://localhost:7285/api/Trip';
  private readonly http = inject(HttpClient);

  getAll(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl);
  }
}
