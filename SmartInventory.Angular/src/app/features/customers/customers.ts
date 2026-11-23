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

import { CustomerService } from '../../core/services/customer.service';
import { AuthService } from '../../core/services/auth.service';

import { Customer } from '../../core/models/customer/customer';
import { CreateCustomer } from '../../core/models/customer/create-customer';
import { UpdateCustomer } from '../../core/models/customer/update-customer';

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './customers.html',
  styleUrl: './customers.scss'
})
export class Customers implements OnInit {

  private customerService = inject(CustomerService);
  private authService = inject(AuthService);

  customers = signal<Customer[]>([]);

  isLoading = signal(false);
  isSaving = signal(false);

  searchTerm = signal('');

  showForm = signal(false);
  isEditMode = signal(false);

  errorMessage = signal('');
  successMessage = signal('');

  selectedCustomerId: number | null = null;

  formData = {
    customerName: '',
    email: '',
    phone: '',
    address: ''
  };

  filteredCustomers = computed(() => {
    const search = this.searchTerm().toLowerCase().trim();

    if (!search) {
      return this.customers();
    }

    return this.customers().filter(customer =>
      customer.customerName.toLowerCase().includes(search) ||
      (customer.email ?? '').toLowerCase().includes(search) ||
      (customer.phone ?? '').toLowerCase().includes(search) ||
      (customer.address ?? '').toLowerCase().includes(search)
    );
  });

  totalCustomers = computed(() => this.customers().length);

  customersWithEmail = computed(() =>
    this.customers().filter(x => !!x.email).length
  );

  customersWithoutEmail = computed(() =>
    this.customers().filter(x => !x.email).length
  );

  get canManage(): boolean {
    return this.authService.hasRole(['Admin', 'Manager', 'Staff']);
  }

  get canEdit(): boolean {
    return this.authService.hasRole(['Admin', 'Manager']);
  }

  get isAdmin(): boolean {
    return this.authService.hasRole(['Admin']);
  }

  ngOnInit(): void {
    this.loadCustomers();
  }

  loadCustomers(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.customerService.getAll().subscribe({
      next: (data) => {
        this.customers.set(data);
        this.isLoading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(
          error.error?.message || 'Unable to load customers.'
        );
        this.isLoading.set(false);
      }
    });
  }

  openAddForm(): void {
    this.resetForm();
    this.isEditMode.set(false);
    this.showForm.set(true);
  }

  openEditForm(customer: Customer): void {
    this.resetForm();

    this.selectedCustomerId = customer.customerId;

    this.formData = {
      customerName: customer.customerName,
      email: customer.email ?? '',
      phone: customer.phone ?? '',
      address: customer.address ?? ''
    };

    this.isEditMode.set(true);
    this.showForm.set(true);
  }

  closeForm(): void {
    if (this.isSaving()) {
      return;
    }

    this.showForm.set(false);
    this.resetForm();
  }

  saveCustomer(): void {
    this.clearMessages();

    if (!this.formData.customerName.trim()) {
      this.errorMessage.set('Customer name is required.');
      return;
    }

    const customerData = {
      customerName: this.formData.customerName.trim(),
      email: this.formData.email.trim() || null,
      phone: this.formData.phone.trim() || null,
      address: this.formData.address.trim() || null
    };

    this.isSaving.set(true);

    if (this.isEditMode() && this.selectedCustomerId !== null) {

      const updateData: UpdateCustomer = customerData;

      this.customerService.update(
        this.selectedCustomerId,
        updateData
      ).subscribe({
        next: () => {
          this.successMessage.set('Customer updated successfully.');
          this.showForm.set(false);
          this.resetForm();
          this.isSaving.set(false);
          this.loadCustomers();
        },
        error: (error: HttpErrorResponse) => {
          this.handleSaveError(error);
        }
      });

    } else {

      const createData: CreateCustomer = customerData;

      this.customerService.create(createData).subscribe({
        next: () => {
          this.successMessage.set('Customer added successfully.');
          this.showForm.set(false);
          this.resetForm();
          this.isSaving.set(false);
          this.loadCustomers();
        },
        error: (error: HttpErrorResponse) => {
          this.handleSaveError(error);
        }
      });
    }
  }

  deleteCustomer(customer: Customer): void {
    const confirmed = confirm(
      `Are you sure you want to delete ${customer.customerName}?`
    );

    if (!confirmed) {
      return;
    }

    this.clearMessages();

    this.customerService.delete(customer.customerId).subscribe({
      next: (response) => {
        this.successMessage.set(
          response.message || 'Customer deleted successfully.'
        );

        this.loadCustomers();
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(
          error.error?.message || 'Unable to delete customer.'
        );
      }
    });
  }

  onSearchChange(value: string): void {
    this.searchTerm.set(value);
  }

  resetForm(): void {
    this.selectedCustomerId = null;

    this.formData = {
      customerName: '',
      email: '',
      phone: '',
      address: ''
    };
  }

  clearMessages(): void {
    this.errorMessage.set('');
    this.successMessage.set('');
  }

  private handleSaveError(error: HttpErrorResponse): void {
    this.errorMessage.set(
      error.error?.message ||
      error.error?.title ||
      'Something went wrong. Please try again.'
    );

    this.isSaving.set(false);
  }
}