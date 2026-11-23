
import {
  Component,
  OnInit,
  inject,
  ChangeDetectorRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { SalesOrderService } from '../../core/services/sales-order.service';
import { AuthService } from '../../core/services/auth.service';

import {
  SalesOrder,
  CustomerOption,
  ProductOption,
  CreateSalesOrderItem
} from '../../core/models/sales-orders/sales-order';

interface OrderFormItem {
  productId: number;
  quantity: number;
}

@Component({
  selector: 'app-sales-orders',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './sales-orders.html',
  styleUrl: './sales-orders.scss'
})
export class SalesOrders implements OnInit {

  private salesService = inject(SalesOrderService);
  private authService = inject(AuthService);
  private cdr = inject(ChangeDetectorRef);

  orders: SalesOrder[] = [];
  customers: CustomerOption[] = [];
  products: ProductOption[] = [];

  loading = false;
  saving = false;
  errorMessage = '';
  successMessage = '';

  showForm = false;
  showDetails = false;

  selectedOrder: SalesOrder | null = null;

  searchTerm = '';
  statusFilter = '';

  customerId = 0;
  notes = '';

  formItems: OrderFormItem[] = [
    { productId: 0, quantity: 1 }
  ];

  get canManage(): boolean {
    return this.authService.hasRole(['Admin', 'Manager']);
  }

  get canCreate(): boolean {
    return this.authService.hasRole(['Admin', 'Manager', 'Staff']);
  }

  get filteredOrders(): SalesOrder[] {
    return this.orders.filter(order => {
      const search = this.searchTerm.toLowerCase();

      const matchesSearch =
        order.customerName.toLowerCase().includes(search) ||
        order.salesOrderId.toString().includes(search);

      const matchesStatus =
        !this.statusFilter || order.status === this.statusFilter;

      return matchesSearch && matchesStatus;
    });
  }

  get totalOrders(): number {
    return this.orders.length;
  }

  get pendingOrders(): number {
    return this.orders.filter(x => x.status === 'Pending').length;
  }

  get completedOrders(): number {
    return this.orders.filter(x => x.status === 'Delivered').length;
  }

  get totalSales(): number {
    return this.orders
      .filter(x => x.status !== 'Cancelled')
      .reduce((sum, x) => sum + x.totalAmount, 0);
  }

  ngOnInit(): void {
    this.loadOrders();
    this.loadCustomers();
    this.loadProducts();
  }

  loadOrders(): void {
    this.loading = true;
    this.errorMessage = '';
    this.cdr.markForCheck();

    this.salesService.getAll().subscribe({
      next: response => {
        this.orders = response;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: error => {
        this.errorMessage = this.getError(error);
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  loadCustomers(): void {
    this.salesService.getCustomers().subscribe({
      next: response => {
        this.customers = response;
        this.cdr.markForCheck();
      },
      error: error => {
        this.errorMessage = this.getError(error);
        this.cdr.markForCheck();
      }
    });
  }

  loadProducts(): void {
    this.salesService.getProducts().subscribe({
      next: response => {
        this.products = response.filter(x => x.isActive);
        this.cdr.markForCheck();
      },
      error: error => {
        this.errorMessage = this.getError(error);
        this.cdr.markForCheck();
      }
    });
  }

  openCreateForm(): void {
    this.customerId = 0;
    this.notes = '';
    this.formItems = [{ productId: 0, quantity: 1 }];
    this.errorMessage = '';
    this.successMessage = '';
    this.showForm = true;
  }

  closeForm(): void {
    this.showForm = false;
  }

  addItem(): void {
    this.formItems.push({ productId: 0, quantity: 1 });
  }

  removeItem(index: number): void {
    if (this.formItems.length > 1) {
      this.formItems.splice(index, 1);
    }
  }

  getProductPrice(productId: number): number {
    return this.products.find(x => x.productId === Number(productId))
      ?.sellingPrice ?? 0;
  }

  getItemTotal(item: OrderFormItem): number {
    return this.getProductPrice(item.productId) * item.quantity;
  }

  get estimatedTotal(): number {
    return this.formItems.reduce(
      (sum, item) => sum + this.getItemTotal(item),
      0
    );
  }

  createOrder(): void {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.customerId) {
      this.errorMessage = 'Please select a customer.';
      return;
    }

    if (
      this.formItems.length === 0 ||
      this.formItems.some(
        x => !x.productId || x.quantity <= 0
      )
    ) {
      this.errorMessage = 'Please select products and enter valid quantities.';
      return;
    }

    const productIds = this.formItems.map(x => Number(x.productId));

    if (new Set(productIds).size !== productIds.length) {
      this.errorMessage = 'Duplicate products are not allowed.';
      return;
    }

    const items: CreateSalesOrderItem[] = this.formItems.map(item => ({
      productId: Number(item.productId),
      quantity: Number(item.quantity)
    }));

    this.saving = true;

    this.salesService.create({
      customerId: Number(this.customerId),
      notes: this.notes.trim() || null,
      items
    }).subscribe({
      next: () => {
        this.saving = false;
        this.showForm = false;
        this.successMessage = 'Sales order created successfully.';
        this.cdr.markForCheck();
        this.loadOrders();
      },
      error: error => {
        this.saving = false;
        this.errorMessage = this.getError(error);
        this.cdr.markForCheck();
      }
    });
  }

  viewDetails(order: SalesOrder): void {
    this.salesService.getById(order.salesOrderId).subscribe({
      next: response => {
        this.selectedOrder = response;
        this.showDetails = true;
        this.cdr.markForCheck();
      },
      error: error => {
        this.errorMessage = this.getError(error);
        this.cdr.markForCheck();
      }
    });
  }

  closeDetails(): void {
    this.showDetails = false;
    this.selectedOrder = null;
  }

  confirmOrder(order: SalesOrder): void {
    if (!confirm(`Confirm Sales Order #${order.salesOrderId}? Stock will be deducted from Warehouse ID 1.`)) {
      return;
    }

    this.salesService.confirm(order.salesOrderId).subscribe({
      next: () => {
        this.successMessage = 'Sales order confirmed successfully.';
        this.cdr.markForCheck();
        this.loadOrders();
      },
      error: error => {
        this.errorMessage = this.getError(error);
        this.cdr.markForCheck();
      }
    });
  }

  updateOrderStatus(order: SalesOrder, status: string): void {
    if (!confirm(`Change order #${order.salesOrderId} status to ${status}?`)) {
      return;
    }

    this.salesService.updateStatus(order.salesOrderId, { status }).subscribe({
      next: () => {
        this.successMessage = `Order status updated to ${status}.`;
        this.cdr.markForCheck();
        this.loadOrders();
      },
      error: error => {
        this.errorMessage = this.getError(error);
        this.cdr.markForCheck();
      }
    });
  }

  cancelOrder(order: SalesOrder): void {
    if (!confirm(`Cancel Sales Order #${order.salesOrderId}?`)) {
      return;
    }

    this.salesService.cancel(order.salesOrderId).subscribe({
      next: () => {
        this.successMessage = 'Sales order cancelled successfully.';
        this.cdr.markForCheck();
        this.loadOrders();
      },
      error: error => {
        this.errorMessage = this.getError(error);
        this.cdr.markForCheck();
      }
    });
  }

  getNextStatus(status: string): string | null {
    switch (status) {
      case 'Confirmed': return 'Processing';
      case 'Processing': return 'Shipped';
      case 'Shipped': return 'Delivered';
      default: return null;
    }
  }

  getError(error: any): string {
    return error?.error?.message ||
      error?.error?.title ||
      error?.error ||
      'Something went wrong. Please try again.';
  }
}
