
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';

import {
  StockTransfer,
  CreateStockTransfer
} from '../models/stock-transfer/stock-transfer';

@Injectable({
  providedIn: 'root'
})
export class StockTransferService {

  private http = inject(HttpClient);

  private readonly apiUrl = `${environment.apiBaseUrl}api/StockTransfers`;

  getAll(): Observable<StockTransfer[]> {
    return this.http.get<StockTransfer[]>(this.apiUrl);
  }

  getById(id: number): Observable<StockTransfer> {
    return this.http.get<StockTransfer>(`${this.apiUrl}/${id}`);
  }

  create(data: CreateStockTransfer): Observable<StockTransfer> {
    return this.http.post<StockTransfer>(this.apiUrl, data);
  }

  approve(id: number): Observable<StockTransfer> {
    return this.http.patch<StockTransfer>(
      `${this.apiUrl}/${id}/approve`,
      {}
    );
  }

  dispatch(id: number): Observable<StockTransfer> {
    return this.http.patch<StockTransfer>(
      `${this.apiUrl}/${id}/dispatch`,
      {}
    );
  }

  complete(id: number): Observable<StockTransfer> {
    return this.http.patch<StockTransfer>(
      `${this.apiUrl}/${id}/complete`,
      {}
    );
  }

  cancel(id: number): Observable<void> {
    return this.http.patch<void>(
      `${this.apiUrl}/${id}/cancel`,
      {}
    );
  }
}

