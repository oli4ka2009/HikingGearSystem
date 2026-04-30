import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private readonly baseUrl = 'https://localhost:7285/api/Category';
  private readonly http = inject(HttpClient);

  getAllCategories(): Observable<any[]> {
    return this.http.get<any[]>(this.baseUrl);
  }

  createCategory(data: { name: string; tripId: number }): Observable<any> {
    return this.http.post<any>(this.baseUrl, data);
  }

  updateCategory(id: number, data: { name: string }): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/${id}`, data);
  }

  deleteCategory(id: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/${id}`);
  }
}
