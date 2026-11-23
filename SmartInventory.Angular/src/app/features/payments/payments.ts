
import {
  Component,
  OnInit,
  inject,
  ChangeDetectorRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { PaymentService } from '../../core/services/payment.service';
import { SalesOrderService } from '../../core/services/sales-order.service';
import { AuthService } from '../../core/services/auth.service';

import {
  Payment,
  PaymentSummary
} from '../../core/models/payments/payment';

import { SalesOrder } from '../../core/models/sales-orders/sales-order';

@Component({
  selector: 'app-payments',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './payments.html',
  styleUrl: './payments.scss'
})
export class Payments implements OnInit {

  private paymentService = inject(PaymentService);
  private salesOrderService = inject(SalesOrderService);
  private authService = inject(AuthService);
  private cdr = inject(ChangeDetectorRef);

  payments: Payment[] = [];
  orders: SalesOrder[] = [];

  loading = false;
  saving = false;

  errorMessage = '';
  successMessage = '';

  showForm = false;
  showSummary = false;

  selectedSummary: PaymentSummary | null = null;

  searchTerm = '';
  statusFilter = '';

  salesOrderId = 0;
  amount: number | null = null;
  paymentMethod = '';
  transactionReference = '';

  selectedOrder: SalesOrder | null = null;

  get canManage(): boolean {
    return this.authService.hasRole(['Admin', 'Manager']);
  }

  get canCreate(): boolean {
    return this.authService.hasRole(['Admin', 'Manager', 'Staff']);
  }

  get filteredPayments(): Payment[] {
    return this.payments.filter(payment => {
      const search = this.searchTerm.toLowerCase();

      const matchesSearch =
        payment.paymentId.toString().includes(search) ||
        payment.salesOrderId.toString().includes(search) ||
        payment.paymentMethod.toLowerCase().includes(search);

      const matchesStatus =
        !this.statusFilter || payment.status === this.statusFilter;

      return matchesSearch && matchesStatus;
    });
  }

  get totalPayments(): number {
    return this.payments.length;
  }

  get paidAmount(): number {
    return this.payments
      .filter(x => x.status === 'Paid')
      .reduce((sum, x) => sum + x.amount, 0);
  }

  get pendingPayments(): number {
    return this.payments.filter(x => x.status === 'Pending').length;
  }

  get failedPayments(): number {
    return this.payments.filter(x => x.status === 'Failed').length;
  }

  ngOnInit(): void {
    this.loadPayments();
    this.loadOrders();
  }

  loadPayments(): void {
    this.loading = true;
    this.errorMessage = '';
    this.cdr.markForCheck();

    this.paymentService.getAll().subscribe({
      next: response => {
        this.payments = response;
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

  loadOrders(): void {
    this.salesOrderService.getAll().subscribe({
      next: response => {
        this.orders = response;
        this.cdr.markForCheck();
      },
      error: error => {
        this.errorMessage = this.getError(error);
        this.cdr.markForCheck();
      }
    });
  }

  openCreateForm(): void {
    this.salesOrderId = 0;
    this.amount = null;
    this.paymentMethod = '';
    this.transactionReference = '';
    this.selectedOrder = null;
    this.errorMessage = '';
    this.showForm = true;
  }

  closeForm(): void {
    this.showForm = false;
  }

  onOrderChange(): void {
    this.selectedOrder =
      this.orders.find(x => x.salesOrderId === Number(this.salesOrderId)) ?? null;

    if (this.selectedOrder) {
      this.paymentService.getSummary(this.selectedOrder.salesOrderId).subscribe({
        next: summary => {
          this.amount = summary.remainingAmount > 0
            ? summary.remainingAmount
            : null;

          this.cdr.markForCheck();
        },
        error: error => {
          this.errorMessage = this.getError(error);
          this.cdr.markForCheck();
        }
      });
    } else {
      this.amount = null;
    }
  }

  createPayment(): void {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.salesOrderId) {
      this.errorMessage = 'Please select a sales order.';
      return;
    }

    if (!this.amount || this.amount <= 0) {
      this.errorMessage = 'Enter a valid payment amount.';
      return;
    }

    if (!this.paymentMethod.trim()) {
      this.errorMessage = 'Please enter a payment method.';
      return;
    }

    this.saving = true;

    this.paymentService.create({
      salesOrderId: Number(this.salesOrderId),
      amount: Number(this.amount),
      paymentMethod: this.paymentMethod.trim(),
      transactionReference: this.transactionReference.trim() || null
    }).subscribe({
      next: () => {
        this.saving = false;
        this.showForm = false;
        this.successMessage = 'Payment recorded successfully.';
        this.cdr.markForCheck();
        this.loadPayments();
      },
      error: error => {
        this.saving = false;
        this.errorMessage = this.getError(error);
        this.cdr.markForCheck();
      }
    });
  }

  viewSummary(payment: Payment): void {
    this.paymentService.getSummary(payment.salesOrderId).subscribe({
      next: response => {
        this.selectedSummary = response;
        this.showSummary = true;
        this.cdr.markForCheck();
      },
      error: error => {
        this.errorMessage = this.getError(error);
        this.cdr.markForCheck();
      }
    });
  }

  closeSummary(): void {
    this.showSummary = false;
    this.selectedSummary = null;
  }

  updatePaymentStatus(payment: Payment, status: string): void {
    if (!confirm(`Change Payment #${payment.paymentId} status to ${status}?`)) {
      return;
    }

    this.paymentService.updateStatus(payment.paymentId, { status }).subscribe({
      next: () => {
        this.successMessage = `Payment status updated to ${status}.`;
        this.cdr.markForCheck();
        this.loadPayments();
      },
      error: error => {
        this.errorMessage = this.getError(error);
        this.cdr.markForCheck();
      }
    });
  }

  getError(error: any): string {
    return error?.error?.message ||
      error?.error?.title ||
      error?.error ||
      'Something went wrong. Please try again.';
  }
}
