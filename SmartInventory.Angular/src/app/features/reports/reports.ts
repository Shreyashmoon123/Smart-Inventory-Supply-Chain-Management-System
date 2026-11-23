
import {
  Component,
  ChangeDetectorRef,
  OnInit,
  inject
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { ReportService } from '../../core/services/report.service';

import {
  SalesReport,
  PurchaseReport,
  InventoryReport,
  StockTransactionReport,
  ReportType
} from '../../core/models/reports/report';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reports.html',
  styleUrl: './reports.scss'
})
export class Reports implements OnInit {
  private reportService = inject(ReportService);
  private cdr = inject(ChangeDetectorRef);

  activeReport: ReportType = 'sales';

  salesReports: SalesReport[] = [];
  purchaseReports: PurchaseReport[] = [];
  inventoryReports: InventoryReport[] = [];
  transactionReports: StockTransactionReport[] = [];

  fromDate = '';
  toDate = '';
  searchTerm = '';

  loading = false;
  errorMessage = '';

  ngOnInit(): void {
    this.loadReport();
  }

  setReport(report: ReportType): void {
    this.activeReport = report;
    this.searchTerm = '';
    this.loadReport();
  }

  loadReport(): void {
    this.loading = true;
    this.errorMessage = '';

    switch (this.activeReport) {
      case 'sales':
        this.reportService
          .getSalesReport(this.fromDate, this.toDate)
          .subscribe({
            next: data => {
              this.salesReports = data ?? [];
              this.finishLoading();
            },
            error: error => this.handleError(error)
          });
        break;

      case 'purchases':
        this.reportService
          .getPurchaseReport(this.fromDate, this.toDate)
          .subscribe({
            next: data => {
              this.purchaseReports = data ?? [];
              this.finishLoading();
            },
            error: error => this.handleError(error)
          });
        break;

      case 'inventory':
        this.reportService
          .getInventoryReport()
          .subscribe({
            next: data => {
              this.inventoryReports = data ?? [];
              this.finishLoading();
            },
            error: error => this.handleError(error)
          });
        break;

      case 'stock-transactions':
        this.reportService
          .getStockTransactionReport(this.fromDate, this.toDate)
          .subscribe({
            next: data => {
              this.transactionReports = data ?? [];
              this.finishLoading();
            },
            error: error => this.handleError(error)
          });
        break;
    }
  }

  applyFilters(): void {
    if (
      this.fromDate &&
      this.toDate &&
      this.fromDate > this.toDate
    ) {
      this.errorMessage = 'From date cannot be after To date.';
      return;
    }

    this.loadReport();
  }

  resetFilters(): void {
    this.fromDate = '';
    this.toDate = '';
    this.searchTerm = '';
    this.loadReport();
  }

  private finishLoading(): void {
    this.loading = false;
    this.cdr.markForCheck();
  }

  private handleError(error: any): void {
    this.loading = false;
    this.errorMessage =
      error?.error?.message ||
      error?.error?.title ||
      'Unable to load report data. Please try again.';

    this.cdr.markForCheck();
  }

  get filteredSales(): SalesReport[] {
    const term = this.searchTerm.toLowerCase().trim();

    if (!term) return this.salesReports;

    return this.salesReports.filter(item =>
      `${item.salesOrderId} ${item.customerName} ${item.status}`
        .toLowerCase()
        .includes(term)
    );
  }

  get filteredPurchases(): PurchaseReport[] {
    const term = this.searchTerm.toLowerCase().trim();

    if (!term) return this.purchaseReports;

    return this.purchaseReports.filter(item =>
      `${item.purchaseOrderId} ${item.supplierName} ${item.warehouseName} ${item.status}`
        .toLowerCase()
        .includes(term)
    );
  }

  get filteredInventory(): InventoryReport[] {
    const term = this.searchTerm.toLowerCase().trim();

    if (!term) return this.inventoryReports;

    return this.inventoryReports.filter(item =>
      `${item.productName} ${item.sku} ${item.warehouseName}`
        .toLowerCase()
        .includes(term)
    );
  }

  get filteredTransactions(): StockTransactionReport[] {
    const term = this.searchTerm.toLowerCase().trim();

    if (!term) return this.transactionReports;

    return this.transactionReports.filter(item =>
      `${item.stockTransactionId} ${item.productName} ${item.warehouseName} ${item.transactionType}`
        .toLowerCase()
        .includes(term)
    );
  }

  get totalSales(): number {
    return this.salesReports.reduce(
      (total, item) => total + item.totalAmount,
      0
    );
  }

  get totalPurchases(): number {
    return this.purchaseReports.reduce(
      (total, item) => total + item.totalAmount,
      0
    );
  }

  get lowStockCount(): number {
    return this.inventoryReports.filter(item => item.isLowStock).length;
  }

  get totalInventoryQuantity(): number {
    return this.inventoryReports.reduce(
      (total, item) => total + item.quantity,
      0
    );
  }

  get reportTitle(): string {
    const titles: Record<ReportType, string> = {
      sales: 'Sales Report',
      purchases: 'Purchase Report',
      inventory: 'Inventory Report',
      'stock-transactions': 'Stock Transactions'
    };

    return titles[this.activeReport];
  }

  get showDateFilters(): boolean {
    return this.activeReport !== 'inventory';
  }
}
