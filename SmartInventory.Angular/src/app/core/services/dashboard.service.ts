import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';

import type { DashboardSummary } from '../models/dashboard/dashboard-summary';
import type { LowStockProduct } from '../models/dashboard/low-stock-product';
import type { RecentTransaction } from '../models/dashboard/recent-transaction';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private http = inject(HttpClient);

  private readonly apiUrl = `${environment.apiBaseUrl}api/Dashboard`;

  getSummary(): Observable<DashboardSummary> {
    return this.http.get<DashboardSummary>(`${this.apiUrl}/summary`);
  }

  getLowStock(): Observable<LowStockProduct[]> {
    return this.http.get<LowStockProduct[]>(`${this.apiUrl}/low-stock`);
  }

  getRecentTransactions(): Observable<RecentTransaction[]> {
    return this.http.get<RecentTransaction[]>(
      `${this.apiUrl}/recent-transactions`
    );
  }
}