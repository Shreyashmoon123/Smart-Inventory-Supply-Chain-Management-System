
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';

import type { Category } from '../models/category/category';
import type { CreateCategory } from '../models/category/create-category';
import type { UpdateCategory } from '../models/category/update-category';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private http = inject(HttpClient);

  private readonly apiUrl = `${environment.apiBaseUrl}api/Category`;

  getAll(): Observable<Category[]> {
    return this.http.get<Category[]>(this.apiUrl);
  }

  getById(id: number): Observable<Category> {
    return this.http.get<Category>(`${this.apiUrl}/${id}`);
  }

  create(data: CreateCategory): Observable<Category> {
    return this.http.post<Category>(this.apiUrl, data);
  }

  update(id: number, data: UpdateCategory): Observable<Category> {
    return this.http.put<Category>(`${this.apiUrl}/${id}`, data);
  }

  delete(id: number): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.apiUrl}/${id}`);
  }
}
