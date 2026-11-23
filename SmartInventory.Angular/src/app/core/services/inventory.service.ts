import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';

import { Inventory } from '../models/inventory/inventory';
import { StockTransaction } from '../models/inventory/stock-transaction';
import { StockIn } from '../models/inventory/stock-in';
import { StockOut } from '../models/inventory/stock-out';
import { AdjustStock } from '../models/inventory/adjust-stock';

@Injectable({
  providedIn: 'root'
})
export class InventoryService {

  private http = inject(HttpClient);

  private apiUrl = `${environment.apiBaseUrl}api/Inventory`;

  getAll(): Observable<Inventory[]> {
    return this.http.get<Inventory[]>(this.apiUrl);
  }

  getById(id: number): Observable<Inventory> {
    return this.http.get<Inventory>(`${this.apiUrl}/${id}`);
  }

  getByProduct(productId: number): Observable<Inventory[]> {
    return this.http.get<Inventory[]>(
      `${this.apiUrl}/product/${productId}`
    );
  }

  getByWarehouse(warehouseId: number): Observable<Inventory[]> {
    return this.http.get<Inventory[]>(
      `${this.apiUrl}/warehouse/${warehouseId}`
    );
  }

  getLowStock(): Observable<Inventory[]> {
    return this.http.get<Inventory[]>(`${this.apiUrl}/low-stock`);
  }

  getTransactions(
    productId?: number | null,
    warehouseId?: number | null
  ): Observable<StockTransaction[]> {

    let params = new HttpParams();

    if (productId) {
      params = params.set('productId', productId);
    }

    if (warehouseId) {
      params = params.set('warehouseId', warehouseId);
    }

    return this.http.get<StockTransaction[]>(
      `${this.apiUrl}/transactions`,
      { params }
    );
  }

  stockIn(request: StockIn): Observable<Inventory> {
    return this.http.post<Inventory>(
      `${this.apiUrl}/stock-in`,
      request
    );
  }

  stockOut(request: StockOut): Observable<Inventory> {
    return this.http.post<Inventory>(
      `${this.apiUrl}/stock-out`,
      request
    );
  }

  adjustStock(request: AdjustStock): Observable<Inventory> {
    return this.http.post<Inventory>(
      `${this.apiUrl}/adjust`,
      request
    );
  }
}