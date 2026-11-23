
import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { finalize } from 'rxjs';

import { DashboardService } from '../../core/services/dashboard.service';

import type { DashboardSummary } from '../../core/models/dashboard/dashboard-summary';
import type { LowStockProduct } from '../../core/models/dashboard/low-stock-product';
import type { RecentTransaction } from '../../core/models/dashboard/recent-transaction';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class Dashboard implements OnInit {
  private dashboardService = inject(DashboardService);

  summary = signal<DashboardSummary | null>(null);
  lowStockProducts = signal<LowStockProduct[]>([]);
  recentTransactions = signal<RecentTransaction[]>([]);

  isSummaryLoading = signal(true);
  isLowStockLoading = signal(true);
  isTransactionsLoading = signal(true);

  errorMessage = signal('');

  ngOnInit(): void {
  console.log('Dashboard initialized');

  this.loadDashboard();

  setTimeout(() => {
    console.log('Dashboard state after 2 seconds:', {
      summary: this.summary(),
      lowStock: this.lowStockProducts(),
      transactions: this.recentTransactions(),
      summaryLoading: this.isSummaryLoading(),
      lowStockLoading: this.isLowStockLoading(),
      transactionsLoading: this.isTransactionsLoading()
    });
  }, 2000);
}
  loadDashboard(): void {
    this.errorMessage.set('');

    this.isSummaryLoading.set(true);
    this.isLowStockLoading.set(true);
    this.isTransactionsLoading.set(true);

    this.dashboardService.getSummary()
      .pipe(finalize(() => this.isSummaryLoading.set(false)))
      .subscribe({
        next: data => this.summary.set(data),
        error: error => {
          console.error('Summary Error:', error);
          this.errorMessage.set('Unable to load dashboard summary.');
        }
      });

    this.dashboardService.getLowStock()
      .pipe(finalize(() => this.isLowStockLoading.set(false)))
      .subscribe({
        next: data => this.lowStockProducts.set(data),
        error: error => {
          console.error('Low Stock Error:', error);
          this.errorMessage.set('Unable to load low-stock products.');
        }
      });

    this.dashboardService.getRecentTransactions()
      .pipe(finalize(() => this.isTransactionsLoading.set(false)))
      .subscribe({
        next: data => this.recentTransactions.set(data),
        error: error => {
          console.error('Transactions Error:', error);
          this.errorMessage.set('Unable to load recent transactions.');
        }
      });
  }

  refreshDashboard(): void {
    this.loadDashboard();
  }
}
