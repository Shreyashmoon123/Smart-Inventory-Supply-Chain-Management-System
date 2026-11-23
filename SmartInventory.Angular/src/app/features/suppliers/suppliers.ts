import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';

import { Supplier } from '../../core/models/supplier/supplier';
import { CreateSupplier } from '../../core/models/supplier/create-supplier';
import { UpdateSupplier } from '../../core/models/supplier/update-supplier';

import { SupplierService } from '../../core/services/supplier.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-suppliers',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './suppliers.html',
  styleUrl: './suppliers.scss'
})
export class Suppliers {
  private supplierService = inject(SupplierService);
  private authService = inject(AuthService);

  suppliers = signal<Supplier[]>([]);

  loading = signal(false);
  saving = signal(false);

  showForm = signal(false);
  editingSupplierId = signal<number | null>(null);

  searchTerm = signal('');

  errorMessage = signal('');
  successMessage = signal('');

  formData = {
    supplierName: '',
    email: '',
    phone: '',
    address: '',
    isActive: true
  };

  filteredSuppliers = computed(() => {
    const search = this.searchTerm().trim().toLowerCase();

    if (!search) {
      return this.suppliers();
    }

    return this.suppliers().filter(supplier =>
      supplier.supplierName.toLowerCase().includes(search) ||
      (supplier.email ?? '').toLowerCase().includes(search) ||
      (supplier.phone ?? '').toLowerCase().includes(search)
    );
  });

  get isAdmin(): boolean {
    return this.authService.hasRole(['Admin']);
  }

  get canManage(): boolean {
    return this.authService.hasRole(['Admin', 'Manager']);
  }

  constructor() {
    this.loadSuppliers();
  }

  loadSuppliers(): void {
    this.loading.set(true);
    this.errorMessage.set('');

    this.supplierService.getAll().subscribe({
      next: (data) => {
        this.suppliers.set(data);
        this.loading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(
          error.error?.message || 'Unable to load suppliers.'
        );
        this.loading.set(false);
      }
    });
  }

  openAddForm(): void {
    this.resetForm();
    this.editingSupplierId.set(null);
    this.showForm.set(true);
  }

  openEditForm(supplier: Supplier): void {
    this.formData = {
      supplierName: supplier.supplierName,
      email: supplier.email ?? '',
      phone: supplier.phone ?? '',
      address: supplier.address ?? '',
      isActive: supplier.isActive
    };

    this.editingSupplierId.set(supplier.supplierId);
    this.errorMessage.set('');
    this.successMessage.set('');
    this.showForm.set(true);
  }

  closeForm(): void {
    this.showForm.set(false);
    this.resetForm();
  }

  resetForm(): void {
    this.formData = {
      supplierName: '',
      email: '',
      phone: '',
      address: '',
      isActive: true
    };
  }

  saveSupplier(): void {
    if (!this.formData.supplierName.trim()) {
      this.errorMessage.set('Supplier name is required.');
      return;
    }

    this.saving.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');

    const supplierName = this.formData.supplierName.trim();

    const email = this.formData.email.trim() || null;
    const phone = this.formData.phone.trim() || null;
    const address = this.formData.address.trim() || null;

    const editingId = this.editingSupplierId();

    if (editingId !== null) {
      const request: UpdateSupplier = {
        supplierName,
        email,
        phone,
        address,
        isActive: this.formData.isActive
      };

      this.supplierService.update(editingId, request).subscribe({
        next: () => {
          this.saving.set(false);
          this.showForm.set(false);
          this.successMessage.set('Supplier updated successfully.');
          this.loadSuppliers();
        },
        error: (error: HttpErrorResponse) => {
          this.handleSaveError(error);
        }
      });
    } else {
      const request: CreateSupplier = {
        supplierName,
        email,
        phone,
        address
      };

      this.supplierService.create(request).subscribe({
        next: () => {
          this.saving.set(false);
          this.showForm.set(false);
          this.successMessage.set('Supplier added successfully.');
          this.loadSuppliers();
        },
        error: (error: HttpErrorResponse) => {
          this.handleSaveError(error);
        }
      });
    }
  }

  deleteSupplier(supplier: Supplier): void {
    const confirmed = confirm(
      `Are you sure you want to delete "${supplier.supplierName}"?`
    );

    if (!confirmed) {
      return;
    }

    this.errorMessage.set('');
    this.successMessage.set('');

    this.supplierService.delete(supplier.supplierId).subscribe({
      next: (response) => {
        this.successMessage.set(
          response.message || 'Supplier deleted successfully.'
        );
        this.loadSuppliers();
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(
          error.error?.message || 'Unable to delete supplier.'
        );
      }
    });
  }

  private handleSaveError(error: HttpErrorResponse): void {
    this.saving.set(false);

    this.errorMessage.set(
      error.error?.message ||
      error.error?.title ||
      'Something went wrong. Please try again.'
    );
  }
}