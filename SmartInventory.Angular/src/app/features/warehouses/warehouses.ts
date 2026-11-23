
import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { Warehouse } from '../../core/models/warehouse/warehouse';
import { CreateWarehouse } from '../../core/models/warehouse/create-warehouse';
import { UpdateWarehouse } from '../../core/models/warehouse/update-warehouse';

import { WarehouseService } from '../../core/services/warehouse.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-warehouses',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './warehouses.html',
  styleUrl: './warehouses.scss'
})
export class Warehouses implements OnInit {

  private warehouseService = inject(WarehouseService);
  private authService = inject(AuthService);

  warehouses = signal<Warehouse[]>([]);
  isLoading = signal(false);
  isSaving = signal(false);

  searchTerm = signal('');

  showForm = signal(false);
  isEditMode = signal(false);

  selectedWarehouseId: number | null = null;

  errorMessage = signal('');
  successMessage = signal('');

  formData = {
    warehouseName: '',
    location: '',
    contactNumber: '',
    isActive: true
  };

  filteredWarehouses = computed(() => {
    const search = this.searchTerm().trim().toLowerCase();

    if (!search) {
      return this.warehouses();
    }

    return this.warehouses().filter(warehouse =>
      warehouse.warehouseName.toLowerCase().includes(search) ||
      (warehouse.location ?? '').toLowerCase().includes(search) ||
      (warehouse.contactNumber ?? '').toLowerCase().includes(search)
    );
  });

  totalWarehouses = computed(() => this.warehouses().length);

  activeWarehouses = computed(() =>
    this.warehouses().filter(w => w.isActive).length
  );

  inactiveWarehouses = computed(() =>
    this.warehouses().filter(w => !w.isActive).length
  );

  get canManage(): boolean {
    return this.authService.hasRole(['Admin', 'Manager']);
  }

  get isAdmin(): boolean {
    return this.authService.hasRole(['Admin']);
  }

  ngOnInit(): void {
    this.loadWarehouses();
  }

  loadWarehouses(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.warehouseService.getAll().subscribe({
      next: data => {
        this.warehouses.set(data);
        this.isLoading.set(false);
      },
      error: error => {
        this.errorMessage.set(
          error.error?.message || 'Unable to load warehouses.'
        );
        this.isLoading.set(false);
      }
    });
  }

  openAddForm(): void {
    this.resetForm();
    this.isEditMode.set(false);
    this.showForm.set(true);
    this.clearMessages();
  }

  openEditForm(warehouse: Warehouse): void {
    this.resetForm();

    this.isEditMode.set(true);
    this.selectedWarehouseId = warehouse.warehouseId;

    this.formData = {
      warehouseName: warehouse.warehouseName,
      location: warehouse.location ?? '',
      contactNumber: warehouse.contactNumber ?? '',
      isActive: warehouse.isActive
    };

    this.showForm.set(true);
    this.clearMessages();
  }

  closeForm(): void {
    this.showForm.set(false);
    this.resetForm();
  }

  saveWarehouse(): void {
    this.clearMessages();

    const warehouseName = this.formData.warehouseName.trim();

    if (!warehouseName) {
      this.errorMessage.set('Warehouse name is required.');
      return;
    }

    this.isSaving.set(true);

    if (this.isEditMode() && this.selectedWarehouseId !== null) {
      const request: UpdateWarehouse = {
        warehouseName,
        location: this.formData.location.trim() || null,
        contactNumber: this.formData.contactNumber.trim() || null,
        isActive: this.formData.isActive
      };

      this.warehouseService
        .update(this.selectedWarehouseId, request)
        .subscribe({
          next: () => {
            this.isSaving.set(false);
            this.showForm.set(false);
            this.resetForm();
            this.successMessage.set('Warehouse updated successfully.');
            this.loadWarehouses();
          },
          error: error => {
            this.isSaving.set(false);
            this.errorMessage.set(
              error.error?.message || 'Unable to update warehouse.'
            );
          }
        });

    } else {
      const request: CreateWarehouse = {
        warehouseName,
        location: this.formData.location.trim() || null,
        contactNumber: this.formData.contactNumber.trim() || null
      };

      this.warehouseService.create(request).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.showForm.set(false);
          this.resetForm();
          this.successMessage.set('Warehouse added successfully.');
          this.loadWarehouses();
        },
        error: error => {
          this.isSaving.set(false);
          this.errorMessage.set(
            error.error?.message || 'Unable to create warehouse.'
          );
        }
      });
    }
  }

  deleteWarehouse(warehouse: Warehouse): void {
    const confirmed = confirm(
      `Are you sure you want to delete "${warehouse.warehouseName}"?`
    );

    if (!confirmed) {
      return;
    }

    this.clearMessages();

    this.warehouseService.delete(warehouse.warehouseId).subscribe({
      next: response => {
        this.successMessage.set(
          response.message || 'Warehouse deleted successfully.'
        );
        this.loadWarehouses();
      },
      error: error => {
        this.errorMessage.set(
          error.error?.message || 'Unable to delete warehouse.'
        );
      }
    });
  }

  onSearchChange(value: string): void {
    this.searchTerm.set(value);
  }

  private resetForm(): void {
    this.selectedWarehouseId = null;

    this.formData = {
      warehouseName: '',
      location: '',
      contactNumber: '',
      isActive: true
    };
  }

  private clearMessages(): void {
    this.errorMessage.set('');
    this.successMessage.set('');
  }
}
