
import {
  Component,
  OnInit,
  inject,
  ChangeDetectorRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { PurchaseOrderService } from '../../core/services/purchase-order.service';
import { ProductService } from '../../core/services/product.service';
import { SupplierService } from '../../core/services/supplier.service';
import { WarehouseService } from '../../core/services/warehouse.service';
import { AuthService } from '../../core/services/auth.service';

import { PurchaseOrder } from '../../core/models/purchase-order/purchase-order';
import { CreatePurchaseOrder } from '../../core/models/purchase-order/create-purchase-order';
import { UpdatePurchaseOrder } from '../../core/models/purchase-order/update-purchase-order';
import { ReceivePurchaseOrder } from '../../core/models/purchase-order/receive-purchase-order';

import { Product } from '../../core/models/product/product';
import { Supplier } from '../../core/models/supplier/supplier';
import { Warehouse } from '../../core/models/warehouse/warehouse';

interface PurchaseOrderFormItem {
  productId: number;
  quantity: number;
  unitPrice: number;
}

interface ReceiveFormItem {
  purchaseOrderItemId: number;
  productName: string;
  sku: string;
  orderedQuantity: number;
  receivedQuantity: number;
  remainingQuantity: number;
  receiveNow: number;
}

@Component({
  selector: 'app-purchase-orders',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './purchase-orders.html',
  styleUrl: './purchase-orders.scss'
})
export class PurchaseOrders implements OnInit {

  private purchaseOrderService = inject(PurchaseOrderService);
  private productService = inject(ProductService);
  private supplierService = inject(SupplierService);
  private warehouseService = inject(WarehouseService);
  private authService = inject(AuthService);
  private cdr = inject(ChangeDetectorRef);

  purchaseOrders: PurchaseOrder[] = [];
  products: Product[] = [];
  suppliers: Supplier[] = [];
  warehouses: Warehouse[] = [];

  isLoading = false;
  isSaving = false;

  searchTerm = '';
  selectedStatus = 'All';

  showModal = false;
  modalMode: 'create' | 'edit' | 'details' | 'receive' = 'create';

  selectedOrder: PurchaseOrder | null = null;

  errorMessage = '';
  successMessage = '';

  form = {
    supplierId: 0,
    warehouseId: 0,
    expectedDeliveryDate: '',
    notes: ''
  };

  formItems: PurchaseOrderFormItem[] = [];

  receiveItems: ReceiveFormItem[] = [];
  receiveNotes = '';

  ngOnInit(): void {
    this.loadInitialData();
  }

  get canManage(): boolean {
    return this.authService.hasRole(['Admin', 'Manager']);
  }

  get filteredOrders(): PurchaseOrder[] {
    const search = this.searchTerm.trim().toLowerCase();

    return this.purchaseOrders.filter(order => {
      const matchesSearch =
        !search ||
        order.purchaseOrderId.toString().includes(search) ||
        order.supplierName.toLowerCase().includes(search) ||
        order.warehouseName.toLowerCase().includes(search);

      const matchesStatus =
        this.selectedStatus === 'All' ||
        this.getStatusText(order.status) === this.selectedStatus;

      return matchesSearch && matchesStatus;
    });
  }

  get totalOrders(): number {
    return this.purchaseOrders.length;
  }

  get pendingOrders(): number {
    return this.purchaseOrders.filter(
      order => this.getStatusText(order.status) === 'Pending'
    ).length;
  }

  get approvedOrders(): number {
    return this.purchaseOrders.filter(
      order => this.getStatusText(order.status) === 'Approved'
    ).length;
  }

  get totalPurchaseValue(): number {
    return this.purchaseOrders.reduce(
      (total, order) => total + order.totalAmount,
      0
    );
  }

  loadInitialData(): void {
    this.loadPurchaseOrders();

    this.productService.getAll().subscribe({
      next: data => {
        this.products = data.filter(p => p.isActive);
        this.cdr.markForCheck();
      },
      error: () => {
        this.showError('Unable to load products.');
        this.cdr.markForCheck();
      }
    });

    this.supplierService.getAll().subscribe({
      next: data => {
        this.suppliers = data.filter(s => s.isActive);
        this.cdr.markForCheck();
      },
      error: () => {
        this.showError('Unable to load suppliers.');
        this.cdr.markForCheck();
      }
    });

    this.warehouseService.getAll().subscribe({
      next: data => {
        this.warehouses = data.filter(w => w.isActive);
        this.cdr.markForCheck();
      },
      error: () => {
        this.showError('Unable to load warehouses.');
        this.cdr.markForCheck();
      }
    });
  }

  loadPurchaseOrders(): void {
    this.isLoading = true;
    this.cdr.markForCheck();

    this.purchaseOrderService.getAll().subscribe({
      next: data => {
        this.purchaseOrders = data;
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: error => {
        this.isLoading = false;
        this.showError(
          this.getErrorMessage(error, 'Unable to load purchase orders.')
        );
        this.cdr.markForCheck();
      }
    });
  }

  openCreateModal(): void {
    this.clearMessages();

    this.modalMode = 'create';
    this.selectedOrder = null;

    this.form = {
      supplierId: 0,
      warehouseId: 0,
      expectedDeliveryDate: '',
      notes: ''
    };

    this.formItems = [
      {
        productId: 0,
        quantity: 1,
        unitPrice: 0
      }
    ];

    this.showModal = true;
  }

  openEditModal(order: PurchaseOrder): void {
    if (this.getStatusText(order.status) !== 'Pending') {
      return;
    }

    this.clearMessages();

    this.modalMode = 'edit';
    this.selectedOrder = order;

    this.form = {
      supplierId: order.supplierId,
      warehouseId: order.warehouseId,
      expectedDeliveryDate: order.expectedDeliveryDate
        ? order.expectedDeliveryDate.substring(0, 10)
        : '',
      notes: order.notes ?? ''
    };

    this.formItems = order.items.map(item => ({
      productId: item.productId,
      quantity: item.quantity,
      unitPrice: item.unitPrice
    }));

    this.showModal = true;
  }

  addProductRow(): void {
    this.formItems.push({
      productId: 0,
      quantity: 1,
      unitPrice: 0
    });
  }

  removeProductRow(index: number): void {
    if (this.formItems.length > 1) {
      this.formItems.splice(index, 1);
    }
  }

  onProductChange(item: PurchaseOrderFormItem): void {
    const product = this.products.find(
      p => p.productId === Number(item.productId)
    );

    if (product) {
      item.unitPrice = product.purchasePrice;
    }
  }

  get formTotal(): number {
    return this.formItems.reduce(
      (total, item) =>
        total + (Number(item.quantity) * Number(item.unitPrice)),
      0
    );
  }

  savePurchaseOrder(): void {
    this.clearMessages();

    if (!this.form.supplierId || !this.form.warehouseId) {
      this.showError('Please select a supplier and warehouse.');
      return;
    }

    if (this.formItems.length === 0) {
      this.showError('Please add at least one product.');
      return;
    }

    const productIds = this.formItems.map(item => Number(item.productId));

    if (productIds.some(id => !id)) {
      this.showError('Please select a product for every row.');
      return;
    }

    if (new Set(productIds).size !== productIds.length) {
      this.showError('Duplicate products are not allowed in one purchase order.');
      return;
    }

    if (this.formItems.some(
      item => Number(item.quantity) <= 0 || Number(item.unitPrice) <= 0
    )) {
      this.showError('Quantity and unit price must be greater than zero.');
      return;
    }

    const expectedDate = this.form.expectedDeliveryDate
      ? `${this.form.expectedDeliveryDate}T00:00:00`
      : null;

    const payload: CreatePurchaseOrder = {
      supplierId: Number(this.form.supplierId),
      warehouseId: Number(this.form.warehouseId),
      expectedDeliveryDate: expectedDate,
      notes: this.form.notes.trim() || null,
      items: this.formItems.map(item => ({
        productId: Number(item.productId),
        quantity: Number(item.quantity),
        unitPrice: Number(item.unitPrice)
      }))
    };

    this.isSaving = true;

    if (this.modalMode === 'edit' && this.selectedOrder) {
      const updatePayload: UpdatePurchaseOrder = payload;

      this.purchaseOrderService.update(
        this.selectedOrder.purchaseOrderId,
        updatePayload
      ).subscribe({
        next: () => this.onSaveSuccess('Purchase order updated successfully.'),
        error: error => this.onSaveError(error)
      });

      return;
    }

    this.purchaseOrderService.create(payload).subscribe({
      next: () => this.onSaveSuccess('Purchase order created successfully.'),
      error: error => this.onSaveError(error)
    });
  }

  approveOrder(order: PurchaseOrder): void {
    if (!confirm(`Approve Purchase Order #${order.purchaseOrderId}?`)) {
      return;
    }

    this.clearMessages();

    this.purchaseOrderService.approve(order.purchaseOrderId).subscribe({
      next: () => {
        this.showSuccess('Purchase order approved successfully.');
        this.cdr.markForCheck();
        this.loadPurchaseOrders();
      },
      error: error => {
        this.showError(
          this.getErrorMessage(error, 'Unable to approve purchase order.')
        );
        this.cdr.markForCheck();
      }
    });
  }

  openReceiveModal(order: PurchaseOrder): void {
    this.clearMessages();

    const status = this.getStatusText(order.status);

    if (status !== 'Approved' && status !== 'Partially Received') {
      return;
    }

    this.modalMode = 'receive';
    this.selectedOrder = order;
    this.receiveNotes = '';

    this.receiveItems = order.items
      .filter(item => item.quantity > item.receivedQuantity)
      .map(item => ({
        purchaseOrderItemId: item.purchaseOrderItemId,
        productName: item.productName,
        sku: item.sku,
        orderedQuantity: item.quantity,
        receivedQuantity: item.receivedQuantity,
        remainingQuantity: item.quantity - item.receivedQuantity,
        receiveNow: 0
      }));

    this.showModal = true;
  }

  receivePurchaseOrder(): void {
    this.clearMessages();

    if (!this.selectedOrder) {
      return;
    }

    const items = this.receiveItems
      .filter(item => Number(item.receiveNow) > 0)
      .map(item => ({
        purchaseOrderItemId: item.purchaseOrderItemId,
        receivedQuantity: Number(item.receiveNow)
      }));

    if (items.length === 0) {
      this.showError('Enter a received quantity for at least one product.');
      return;
    }

    const invalidItem = this.receiveItems.find(
      item =>
        Number(item.receiveNow) < 0 ||
        Number(item.receiveNow) > item.remainingQuantity
    );

    if (invalidItem) {
      this.showError(
        `Received quantity cannot exceed the remaining quantity for ${invalidItem.productName}.`
      );
      return;
    }

    const payload: ReceivePurchaseOrder = {
      items,
      notes: this.receiveNotes.trim() || null
    };

    this.isSaving = true;

    this.purchaseOrderService.receive(
      this.selectedOrder.purchaseOrderId,
      payload
    ).subscribe({
      next: () => {
        this.isSaving = false;
        this.showModal = false;
        this.showSuccess('Products received and inventory updated successfully.');
        this.cdr.markForCheck();
        this.loadPurchaseOrders();
      },
      error: error => {
        this.isSaving = false;
        this.showError(
          this.getErrorMessage(error, 'Unable to receive purchase order.')
        );
        this.cdr.markForCheck();
      }
    });
  }

  cancelOrder(order: PurchaseOrder): void {
    if (!confirm(`Cancel Purchase Order #${order.purchaseOrderId}?`)) {
      return;
    }

    this.clearMessages();

    this.purchaseOrderService.cancel(order.purchaseOrderId).subscribe({
      next: () => {
        this.showSuccess('Purchase order cancelled successfully.');
        this.cdr.markForCheck();
        this.loadPurchaseOrders();
      },
      error: error => {
        this.showError(
          this.getErrorMessage(error, 'Unable to cancel purchase order.')
        );
        this.cdr.markForCheck();
      }
    });
  }

  openDetails(order: PurchaseOrder): void {
    this.clearMessages();
    this.selectedOrder = order;
    this.modalMode = 'details';
    this.showModal = true;
  }

  closeModal(): void {
    if (this.isSaving) {
      return;
    }

    this.showModal = false;
    this.selectedOrder = null;
  }

  getStatusText(status: PurchaseOrder['status']): string {
    if (typeof status === 'string') {
      const normalized = status.replace(/\s/g, '').toLowerCase();

      const statusMap: Record<string, string> = {
        pending: 'Pending',
        approved: 'Approved',
        partiallyreceived: 'Partially Received',
        received: 'Received',
        cancelled: 'Cancelled'
      };

      return statusMap[normalized] ?? status;
    }

    const statusMap: Record<number, string> = {
      0: 'Pending',
      1: 'Approved',
      2: 'Partially Received',
      3: 'Received',
      4: 'Cancelled'
    };

    return statusMap[status] ?? 'Unknown';
  }

  getStatusClass(status: PurchaseOrder['status']): string {
    const text = this.getStatusText(status);

    switch (text) {
      case 'Pending': return 'status-pending';
      case 'Approved': return 'status-approved';
      case 'Partially Received': return 'status-partial';
      case 'Received': return 'status-received';
      case 'Cancelled': return 'status-cancelled';
      default: return '';
    }
  }

  canEditOrder(order: PurchaseOrder): boolean {
    return this.canManage && this.getStatusText(order.status) === 'Pending';
  }

  canApproveOrder(order: PurchaseOrder): boolean {
    return this.canManage && this.getStatusText(order.status) === 'Pending';
  }

  canReceiveOrder(order: PurchaseOrder): boolean {
    const status = this.getStatusText(order.status);

    return this.canManage &&
      (status === 'Approved' || status === 'Partially Received');
  }

  canCancelOrder(order: PurchaseOrder): boolean {
    const status = this.getStatusText(order.status);

    return this.canManage &&
      (status === 'Pending' || status === 'Approved');
  }

  private onSaveSuccess(message: string): void {
    this.isSaving = false;
    this.showModal = false;
    this.showSuccess(message);
    this.cdr.markForCheck();
    this.loadPurchaseOrders();
  }

  private onSaveError(error: any): void {
    this.isSaving = false;
    this.showError(
      this.getErrorMessage(error, 'Unable to save purchase order.')
    );
    this.cdr.markForCheck();
  }

  private getErrorMessage(error: any, fallback: string): string {
    return error?.error?.message ||
      error?.error?.title ||
      error?.message ||
      fallback;
  }

  private showError(message: string): void {
    this.errorMessage = message;
    this.successMessage = '';
  }

  private showSuccess(message: string): void {
    this.successMessage = message;
    this.errorMessage = '';
  }

  private clearMessages(): void {
    this.errorMessage = '';
    this.successMessage = '';
  }
}
