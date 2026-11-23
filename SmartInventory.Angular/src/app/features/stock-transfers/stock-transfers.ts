
import { CommonModule } from '@angular/common';
import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';

import { StockTransferService } from '../../core/services/stock-transfer.service';
import { ProductService } from '../../core/services/product.service';
import { WarehouseService } from '../../core/services/warehouse.service';
import { AuthService } from '../../core/services/auth.service';

import { Product } from '../../core/models/product/product';
import { Warehouse } from '../../core/models/warehouse/warehouse';

import {
  StockTransfer,
  CreateStockTransfer,
  CreateStockTransferItem
} from '../../core/models/stock-transfer/stock-transfer';

@Component({
  selector: 'app-stock-transfers',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './stock-transfers.html',
  styleUrl: './stock-transfers.scss'
})
export class StockTransfers implements OnInit {

  private transferService = inject(StockTransferService);
  private productService = inject(ProductService);
  private warehouseService = inject(WarehouseService);
  private authService = inject(AuthService);
  private cdr = inject(ChangeDetectorRef);

  transfers: StockTransfer[] = [];
  warehouses: Warehouse[] = [];
  products: Product[] = [];

  loading = false;
  saving = false;

  showCreateForm = false;
  showDetails = false;

  selectedTransfer: StockTransfer | null = null;

  searchText = '';
  statusFilter = 'All';

  successMessage = '';
  errorMessage = '';

  form: CreateStockTransfer = this.getEmptyForm();

  ngOnInit(): void {
    this.loadTransfers();
    this.loadWarehouses();
    this.loadProducts();
  }

  private getEmptyForm(): CreateStockTransfer {
    return {
      sourceWarehouseId: 0,
      destinationWarehouseId: 0,
      notes: '',
      items: [
        {
          productId: 0,
          quantity: 1
        }
      ]
    };
  }

  get canManage(): boolean {
    return this.authService.hasRole(['Admin', 'Manager']);
  }

  get pendingCount(): number {
    return this.transfers.filter(x => x.status === 'Pending').length;
  }

  get approvedCount(): number {
    return this.transfers.filter(x => x.status === 'Approved').length;
  }

  get inTransitCount(): number {
    return this.transfers.filter(x => x.status === 'InTransit').length;
  }

  get completedCount(): number {
    return this.transfers.filter(x => x.status === 'Completed').length;
  }

  get filteredTransfers(): StockTransfer[] {
    const search = this.searchText.trim().toLowerCase();

    return this.transfers.filter(transfer => {
      const matchesSearch =
        !search ||
        transfer.sourceWarehouseName.toLowerCase().includes(search) ||
        transfer.destinationWarehouseName.toLowerCase().includes(search) ||
        transfer.stockTransferId.toString().includes(search);

      const matchesStatus =
        this.statusFilter === 'All' ||
        transfer.status === this.statusFilter;

      return matchesSearch && matchesStatus;
    });
  }

  loadTransfers(): void {
    this.loading = true;
    this.errorMessage = '';

    this.transferService.getAll().subscribe({
      next: response => {
        this.transfers = response;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: error => {
        this.loading = false;
        this.errorMessage = this.getErrorMessage(error);
        this.cdr.markForCheck();
      }
    });
  }

  private loadWarehouses(): void {
    this.warehouseService.getAll().subscribe({
      next: response => {
        this.warehouses = response.filter(x => x.isActive);
        this.cdr.markForCheck();
      },
      error: error => {
        this.errorMessage = this.getErrorMessage(error);
        this.cdr.markForCheck();
      }
    });
  }

  private loadProducts(): void {
    this.productService.getAll().subscribe({
      next: response => {
        this.products = response.filter(x => x.isActive);
        this.cdr.markForCheck();
      },
      error: error => {
        this.errorMessage = this.getErrorMessage(error);
        this.cdr.markForCheck();
      }
    });
  }

  openCreateForm(): void {
    this.form = this.getEmptyForm();
    this.errorMessage = '';
    this.successMessage = '';
    this.showCreateForm = true;
  }

  closeCreateForm(): void {
    this.showCreateForm = false;
  }

  addItem(): void {
    this.form.items.push({
      productId: 0,
      quantity: 1
    });
  }

  removeItem(index: number): void {
    if (this.form.items.length > 1) {
      this.form.items.splice(index, 1);
    }
  }

  isProductAlreadySelected(productId: number, index: number): boolean {
    if (!productId) {
      return false;
    }

    return this.form.items.some(
      (item, i) => i !== index && item.productId === productId
    );
  }

  createTransfer(): void {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.form.sourceWarehouseId || !this.form.destinationWarehouseId) {
      this.errorMessage = 'Please select both warehouses.';
      return;
    }

    if (this.form.sourceWarehouseId === this.form.destinationWarehouseId) {
      this.errorMessage = 'Source and destination warehouses must be different.';
      return;
    }

    if (!this.form.items.length) {
      this.errorMessage = 'Add at least one product.';
      return;
    }

    const invalidItem = this.form.items.some(
      item => !item.productId || item.quantity <= 0
    );

    if (invalidItem) {
      this.errorMessage = 'Select a product and enter a valid quantity for every item.';
      return;
    }

    const productIds = this.form.items.map(item => item.productId);

    if (new Set(productIds).size !== productIds.length) {
      this.errorMessage = 'Duplicate products are not allowed.';
      return;
    }

    const request: CreateStockTransfer = {
      sourceWarehouseId: Number(this.form.sourceWarehouseId),
      destinationWarehouseId: Number(this.form.destinationWarehouseId),
      notes: this.form.notes?.trim() || null,
      items: this.form.items.map((item: CreateStockTransferItem) => ({
        productId: Number(item.productId),
        quantity: Number(item.quantity)
      }))
    };

    this.saving = true;

    this.transferService.create(request).subscribe({
      next: () => {
        this.saving = false;
        this.showCreateForm = false;
        this.successMessage = 'Stock transfer created successfully.';
        this.form = this.getEmptyForm();
        this.loadTransfers();
        this.cdr.markForCheck();
      },
      error: error => {
        this.saving = false;
        this.errorMessage = this.getErrorMessage(error);
        this.cdr.markForCheck();
      }
    });
  }

  openDetails(transfer: StockTransfer): void {
    this.selectedTransfer = transfer;
    this.showDetails = true;

    this.transferService.getById(transfer.stockTransferId).subscribe({
      next: response => {
        this.selectedTransfer = response;
        this.cdr.markForCheck();
      },
      error: error => {
        this.errorMessage = this.getErrorMessage(error);
        this.cdr.markForCheck();
      }
    });
  }

  closeDetails(): void {
    this.showDetails = false;
    this.selectedTransfer = null;
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Pending':
        return 'status-pending';
      case 'Approved':
        return 'status-approved';
      case 'InTransit':
        return 'status-transit';
      case 'Completed':
        return 'status-completed';
      case 'Cancelled':
        return 'status-cancelled';
      default:
        return '';
    }
  }

  approveTransfer(id: number): void {
    this.performAction(
      this.transferService.approve(id),
      'Stock transfer approved successfully.'
    );
  }

  dispatchTransfer(id: number): void {
    this.performAction(
      this.transferService.dispatch(id),
      'Stock transfer dispatched successfully.'
    );
  }

  completeTransfer(id: number): void {
    this.performAction(
      this.transferService.complete(id),
      'Stock transfer completed successfully.'
    );
  }

  cancelTransfer(id: number): void {
    if (!confirm('Are you sure you want to cancel this stock transfer?')) {
      return;
    }

    this.performAction(
      this.transferService.cancel(id),
      'Stock transfer cancelled successfully.'
    );
  }

  private performAction(
    request: Observable<unknown>,
    message: string
  ): void {
    this.errorMessage = '';
    this.successMessage = '';

    request.subscribe({
      next: () => {
        this.successMessage = message;
        this.loadTransfers();

        if (this.selectedTransfer) {
          this.transferService.getById(
            this.selectedTransfer.stockTransferId
          ).subscribe({
            next: response => {
              this.selectedTransfer = response;
              this.cdr.markForCheck();
            }
          });
        }

        this.cdr.markForCheck();
      },
      error: error => {
        this.errorMessage = this.getErrorMessage(error);
        this.cdr.markForCheck();
      }
    });
  }

  private getErrorMessage(error: any): string {
    return error?.error?.message ||
      error?.error?.title ||
      error?.message ||
      'Something went wrong. Please try again.';
  }
}
