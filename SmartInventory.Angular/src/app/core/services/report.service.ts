
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';

import {
  SalesReport,
  PurchaseReport,
  InventoryReport,
  StockTransactionReport
} from '../models/reports/report';

@Injectable({
  providedIn: 'root'
})
export class ReportService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiBaseUrl}api/Reports`;

  getSalesReport(
    fromDate?: string,
    toDate?: string
  ): Observable<SalesReport[]> {
    return this.http.get<SalesReport[]>(
      `${this.apiUrl}/sales`,
      { params: this.getDateParams(fromDate, toDate) }
    );
  }

  getPurchaseReport(
    fromDate?: string,
    toDate?: string
  ): Observable<PurchaseReport[]> {
    return this.http.get<PurchaseReport[]>(
      `${this.apiUrl}/purchases`,
      { params: this.getDateParams(fromDate, toDate) }
    );
  }

  getInventoryReport(): Observable<InventoryReport[]> {
    return this.http.get<InventoryReport[]>(
      `${this.apiUrl}/inventory`
    );
  }

  getStockTransactionReport(
    fromDate?: string,
    toDate?: string
  ): Observable<StockTransactionReport[]> {
    return this.http.get<StockTransactionReport[]>(
      `${this.apiUrl}/stock-transactions`,
      { params: this.getDateParams(fromDate, toDate) }
    );
  }

  private getDateParams(
    fromDate?: string,
    toDate?: string
  ): HttpParams {
    let params = new HttpParams();

    if (fromDate) {
      params = params.set('fromDate', fromDate);
    }

    if (toDate) {
      params = params.set('toDate', toDate);
    }

    return params;
  }
}
