import {
  Component,
  OnInit,
  inject,
  signal,
  computed
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';

import { InventoryService } from '../../core/services/inventory.service';
import { ProductService } from '../../core/services/product.service';
import { WarehouseService } from '../../core/services/warehouse.service';
import { AuthService } from '../../core/services/auth.service';

import { Inventory as InventoryModel } from '../../core/models/inventory/inventory';
import { StockTransaction } from '../../core/models/inventory/stock-transaction';
import { StockIn } from '../../core/models/inventory/stock-in';
import { StockOut } from '../../core/models/inventory/stock-out';
import { AdjustStock } from '../../core/models/inventory/adjust-stock';

import { Product } from '../../core/models/product/product';
import { Warehouse } from '../../core/models/warehouse/warehouse';

type InventoryView = 'all' | 'low-stock' | 'transactions';
type StockOperation = 'stock-in' | 'stock-out' | 'adjust';

@Component({
  selector: 'app-inventory',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './inventory.html',
  styleUrl: './inventory.scss'
})
export class Inventory implements OnInit {

  private inventoryService = inject(InventoryService);
  private productService = inject(ProductService);
  private warehouseService = inject(WarehouseService);
  private authService = inject(AuthService);

  inventory = signal<InventoryModel[]>([]);
  lowStockItems = signal<InventoryModel[]>([]);
  transactions = signal<StockTransaction[]>([]);

  products = signal<Product[]>([]);
  warehouses = signal<Warehouse[]>([]);

  isLoading = signal(false);
  isSaving = signal(false);

  selectedView = signal<InventoryView>('all');

  searchTerm = signal('');
  selectedProductId = signal<number | null>(null);
  selectedWarehouseId = signal<number | null>(null);

  showForm = signal(false);
  operation = signal<StockOperation>('stock-in');

  errorMessage = signal('');
  successMessage = signal('');

  formData = {
    productId: 0,
    warehouseId: 0,
    quantity: 0,
    newQuantity: 0,
    referenceType: '',
    referenceId: null as number | null,
    notes: ''
  };

  filteredInventory = computed(() => {
    const search = this.searchTerm().toLowerCase().trim();

    return this.inventory().filter(item => {
      const matchesSearch =
        !search ||
        item.productName.toLowerCase().includes(search) ||
        item.sku.toLowerCase().includes(search) ||
        item.warehouseName.toLowerCase().includes(search);

      const matchesProduct =
        !this.selectedProductId() ||
        item.productId === this.selectedProductId();

      const matchesWarehouse =
        !this.selectedWarehouseId() ||
        item.warehouseId === this.selectedWarehouseId();

      return matchesSearch && matchesProduct && matchesWarehouse;
    });
  });

  filteredLowStock = computed(() => {
    const search = this.searchTerm().toLowerCase().trim();

    return this.lowStockItems().filter(item => {
      const matchesSearch =
        !search ||
        item.productName.toLowerCase().includes(search) ||
        item.sku.toLowerCase().includes(search) ||
        item.warehouseName.toLowerCase().includes(search);

      const matchesProduct =
        !this.selectedProductId() ||
        item.productId === this.selectedProductId();

      const matchesWarehouse =
        !this.selectedWarehouseId() ||
        item.warehouseId === this.selectedWarehouseId();

      return matchesSearch && matchesProduct && matchesWarehouse;
    });
  });

  totalQuantity = computed(() =>
    this.inventory().reduce((total, item) => total + item.quantity, 0)
  );

  lowStockCount = computed(() => this.lowStockItems().length);

  totalProducts = computed(() =>
    new Set(this.inventory().map(item => item.productId)).size
  );

  totalWarehouses = computed(() =>
    new Set(this.inventory().map(item => item.warehouseId)).size
  );

  filteredTransactions = computed(() => {
    const search = this.searchTerm().toLowerCase().trim();

    return this.transactions().filter(transaction => {
      return !search ||
        transaction.productName.toLowerCase().includes(search) ||
        transaction.warehouseName.toLowerCase().includes(search) ||
        String(transaction.transactionType).toLowerCase().includes(search) ||
        (transaction.referenceType ?? '').toLowerCase().includes(search);
    });
  });

  get canManageStock(): boolean {
    return this.authService.hasRole(['Admin', 'Manager']);
  }

  get isAdmin(): boolean {
    return this.authService.hasRole(['Admin']);
  }

  ngOnInit(): void {
    this.loadInventory();
    this.loadLowStock();
    this.loadProducts();
    this.loadWarehouses();
    this.loadTransactions();
  }

  loadInventory(): void {
    this.isLoading.set(true);

    this.inventoryService.getAll().subscribe({
      next: data => {
        this.inventory.set(data);
        this.isLoading.set(false);
      },
      error: error => {
        this.handleLoadError(error);
      }
    });
  }

  loadLowStock(): void {
    this.inventoryService.getLowStock().subscribe({
      next: data => this.lowStockItems.set(data),
      error: error => this.errorMessage.set(
        error.error?.message || 'Unable to load low-stock inventory.'
      )
    });
  }

  loadProducts(): void {
    this.productService.getAll().subscribe({
      next: data => this.products.set(data.filter(x => x.isActive)),
      error: () => this.errorMessage.set('Unable to load products.')
    });
  }

  loadWarehouses(): void {
    this.warehouseService.getAll().subscribe({
      next: data => this.warehouses.set(data.filter(x => x.isActive)),
      error: () => this.errorMessage.set('Unable to load warehouses.')
    });
  }

  loadTransactions(): void {
    this.inventoryService.getTransactions(
      this.selectedProductId(),
      this.selectedWarehouseId()
    ).subscribe({
      next: data => this.transactions.set(data),
      error: error => this.errorMessage.set(
        error.error?.message || 'Unable to load transactions.'
      )
    });
  }

  changeView(view: InventoryView): void {
    this.selectedView.set(view);
    this.searchTerm.set('');
  }

  applyFilters(): void {
    this.loadTransactions();
  }

  clearFilters(): void {
    this.selectedProductId.set(null);
    this.selectedWarehouseId.set(null);
    this.searchTerm.set('');
    this.loadTransactions();
  }

  openStockForm(operation: StockOperation): void {
    this.clearMessages();

    this.operation.set(operation);

    this.formData = {
      productId: 0,
      warehouseId: 0,
      quantity: 0,
      newQuantity: 0,
      referenceType: '',
      referenceId: null,
      notes: ''
    };

    this.showForm.set(true);
  }

  closeForm(): void {
    if (this.isSaving()) {
      return;
    }

    this.showForm.set(false);
  }

  saveStock(): void {
    this.clearMessages();

    if (!this.formData.productId || !this.formData.warehouseId) {
      this.errorMessage.set('Please select a product and warehouse.');
      return;
    }

    if (
      this.operation() !== 'adjust' &&
      this.formData.quantity <= 0
    ) {
      this.errorMessage.set('Quantity must be greater than zero.');
      return;
    }

    if (
      this.operation() === 'adjust' &&
      this.formData.newQuantity < 0
    ) {
      this.errorMessage.set('New quantity cannot be negative.');
      return;
    }

    this.isSaving.set(true);

    if (this.operation() === 'stock-in') {
      const request: StockIn = {
        productId: this.formData.productId,
        warehouseId: this.formData.warehouseId,
        quantity: this.formData.quantity,
        referenceType: this.formData.referenceType.trim() || null,
        referenceId: this.formData.referenceId,
        notes: this.formData.notes.trim() || null
      };

      this.inventoryService.stockIn(request).subscribe({
        next: () => this.handleOperationSuccess('Stock added successfully.'),
        error: error => this.handleOperationError(error)
      });

      return;
    }

    if (this.operation() === 'stock-out') {
      const request: StockOut = {
        productId: this.formData.productId,
        warehouseId: this.formData.warehouseId,
        quantity: this.formData.quantity,
        referenceType: this.formData.referenceType.trim() || null,
        referenceId: this.formData.referenceId,
        notes: this.formData.notes.trim() || null
      };

      this.inventoryService.stockOut(request).subscribe({
        next: () => this.handleOperationSuccess('Stock removed successfully.'),
        error: error => this.handleOperationError(error)
      });

      return;
    }

    const request: AdjustStock = {
      productId: this.formData.productId,
      warehouseId: this.formData.warehouseId,
      newQuantity: this.formData.newQuantity,
      notes: this.formData.notes.trim() || null
    };

    this.inventoryService.adjustStock(request).subscribe({
      next: () => this.handleOperationSuccess('Stock adjusted successfully.'),
      error: error => this.handleOperationError(error)
    });
  }

  private handleOperationSuccess(message: string): void {
    this.successMessage.set(message);
    this.showForm.set(false);
    this.isSaving.set(false);

    this.loadInventory();
    this.loadLowStock();
    this.loadTransactions();
  }

  private handleOperationError(error: HttpErrorResponse): void {
    this.errorMessage.set(
      error.error?.message ||
      error.error?.title ||
      'Stock operation failed. Please try again.'
    );

    this.isSaving.set(false);
  }

  private handleLoadError(error: HttpErrorResponse): void {
    this.errorMessage.set(
      error.error?.message || 'Unable to load inventory.'
    );

    this.isLoading.set(false);
  }

  clearMessages(): void {
    this.errorMessage.set('');
    this.successMessage.set('');
  }

  getOperationTitle(): string {
    switch (this.operation()) {
      case 'stock-in':
        return 'Stock In';
      case 'stock-out':
        return 'Stock Out';
      case 'adjust':
        return 'Adjust Stock';
    }
  }
}