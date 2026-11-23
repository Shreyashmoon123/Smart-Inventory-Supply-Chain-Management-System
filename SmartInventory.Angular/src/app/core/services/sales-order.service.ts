
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

import {
  SalesOrder,
  CreateSalesOrder,
  UpdateSalesOrderStatus,
  CustomerOption,
  ProductOption
} from '../models/sales-orders/sales-order';

@Injectable({
  providedIn: 'root'
})
export class SalesOrderService {

  private http = inject(HttpClient);

  private apiUrl = environment.apiBaseUrl;

  getAll(): Observable<SalesOrder[]> {
    return this.http.get<SalesOrder[]>(
      `${this.apiUrl}api/SalesOrders`
    );
  }

  getById(id: number): Observable<SalesOrder> {
    return this.http.get<SalesOrder>(
      `${this.apiUrl}api/SalesOrders/${id}`
    );
  }

  create(data: CreateSalesOrder): Observable<SalesOrder> {
    return this.http.post<SalesOrder>(
      `${this.apiUrl}api/SalesOrders`,
      data
    );
  }

  confirm(id: number): Observable<SalesOrder> {
    return this.http.patch<SalesOrder>(
      `${this.apiUrl}api/SalesOrders/${id}/confirm`,
      {}
    );
  }

  updateStatus(
    id: number,
    data: UpdateSalesOrderStatus
  ): Observable<SalesOrder> {
    return this.http.patch<SalesOrder>(
      `${this.apiUrl}api/SalesOrders/${id}/status`,
      data
    );
  }

  cancel(id: number): Observable<SalesOrder> {
    return this.http.patch<SalesOrder>(
      `${this.apiUrl}api/SalesOrders/${id}/cancel`,
      {}
    );
  }

  getCustomers(): Observable<CustomerOption[]> {
    return this.http.get<CustomerOption[]>(
      `${this.apiUrl}api/Customers`
    );
  }

  getProducts(): Observable<ProductOption[]> {
    return this.http.get<ProductOption[]>(
      `${this.apiUrl}api/Product`
    );
  }
}
