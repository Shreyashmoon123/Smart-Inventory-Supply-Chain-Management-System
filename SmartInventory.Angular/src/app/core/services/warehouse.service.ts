
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { Warehouse } from '../models/warehouse/warehouse';
import { CreateWarehouse } from '../models/warehouse/create-warehouse';
import { UpdateWarehouse } from '../models/warehouse/update-warehouse';

@Injectable({
  providedIn: 'root'
})
export class WarehouseService {

  private http = inject(HttpClient);

  private apiUrl = `${environment.apiBaseUrl}api/Warehouse`;

  getAll(): Observable<Warehouse[]> {
    return this.http.get<Warehouse[]>(this.apiUrl);
  }

  getById(id: number): Observable<Warehouse> {
    return this.http.get<Warehouse>(`${this.apiUrl}/${id}`);
  }

  create(warehouse: CreateWarehouse): Observable<Warehouse> {
    return this.http.post<Warehouse>(this.apiUrl, warehouse);
  }

  update(id: number, warehouse: UpdateWarehouse): Observable<Warehouse> {
    return this.http.put<Warehouse>(`${this.apiUrl}/${id}`, warehouse);
  }

  delete(id: number): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.apiUrl}/${id}`);
  }
}
