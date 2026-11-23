import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';

import { PurchaseOrder } from '../models/purchase-order/purchase-order';
import { CreatePurchaseOrder } from '../models/purchase-order/create-purchase-order';
import { UpdatePurchaseOrder } from '../models/purchase-order/update-purchase-order';
import { ReceivePurchaseOrder } from '../models/purchase-order/receive-purchase-order';

@Injectable({
  providedIn: 'root'
})
export class PurchaseOrderService {

  private http = inject(HttpClient);

  private apiUrl = `${environment.apiBaseUrl}api/PurchaseOrders`;

  getAll(): Observable<PurchaseOrder[]> {
    return this.http.get<PurchaseOrder[]>(this.apiUrl);
  }

  getById(id: number): Observable<PurchaseOrder> {
    return this.http.get<PurchaseOrder>(`${this.apiUrl}/${id}`);
  }

  create(data: CreatePurchaseOrder): Observable<PurchaseOrder> {
    return this.http.post<PurchaseOrder>(this.apiUrl, data);
  }

  update(id: number, data: UpdatePurchaseOrder): Observable<PurchaseOrder> {
    return this.http.put<PurchaseOrder>(`${this.apiUrl}/${id}`, data);
  }

  approve(id: number): Observable<PurchaseOrder> {
    return this.http.patch<PurchaseOrder>(
      `${this.apiUrl}/${id}/approve`,
      {}
    );
  }

  receive(id: number, data: ReceivePurchaseOrder): Observable<PurchaseOrder> {
    return this.http.post<PurchaseOrder>(
      `${this.apiUrl}/${id}/receive`,
      data
    );
  }

  cancel(id: number): Observable<PurchaseOrder> {
    return this.http.patch<PurchaseOrder>(
      `${this.apiUrl}/${id}/cancel`,
      {}
    );
  }
}