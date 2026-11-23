import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { Supplier } from '../models/supplier/supplier';
import { CreateSupplier } from '../models/supplier/create-supplier';
import { UpdateSupplier } from '../models/supplier/update-supplier';

@Injectable({
  providedIn: 'root'
})
export class SupplierService {
  private http = inject(HttpClient);

  private apiUrl = `${environment.apiBaseUrl}api/Supplier`;

  getAll(): Observable<Supplier[]> {
    return this.http.get<Supplier[]>(this.apiUrl);
  }

  getById(id: number): Observable<Supplier> {
    return this.http.get<Supplier>(`${this.apiUrl}/${id}`);
  }

  create(supplier: CreateSupplier): Observable<Supplier> {
    return this.http.post<Supplier>(this.apiUrl, supplier);
  }

  update(id: number, supplier: UpdateSupplier): Observable<Supplier> {
    return this.http.put<Supplier>(`${this.apiUrl}/${id}`, supplier);
  }

  delete(id: number): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.apiUrl}/${id}`);
  }
}