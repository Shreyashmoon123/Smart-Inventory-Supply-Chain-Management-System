import { CommonModule } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { finalize } from 'rxjs';

import { ProductService } from '../../core/services/product.service';
import { CategoryService } from '../../core/services/category.service';
import { AuthService } from '../../core/services/auth.service';

import type { Product } from '../../core/models/product/product';
import type { CreateProduct } from '../../core/models/product/create-product';
import type { UpdateProduct } from '../../core/models/product/update-product';
import type { Category } from '../../core/models/category/category';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './products.html',
  styleUrl: './products.scss'
})
export class Products implements OnInit {
  private productService = inject(ProductService);
  private categoryService = inject(CategoryService);
  private authService = inject(AuthService);

  products = signal<Product[]>([]);
  categories = signal<Category[]>([]);

  isLoading = signal(false);
  isSaving = signal(false);

  errorMessage = signal('');
  successMessage = signal('');

  searchTerm = signal('');
  selectedCategory = signal('');

  showForm = signal(false);
  isEditMode = signal(false);
  editingProductId = signal<number | null>(null);

  productName = '';
  sku = '';
  description = '';
  purchasePrice: number | null = null;
  sellingPrice: number | null = null;
  reorderLevel: number | null = 0;
  categoryId: number | null = null;
  isActive = true;

  canManage = computed(() =>
    this.authService.hasRole(['Admin', 'Manager'])
  );

  canDelete = computed(() =>
    this.authService.hasRole(['Admin'])
  );

  filteredProducts = computed(() => {
    const search = this.searchTerm().trim().toLowerCase();
    const category = this.selectedCategory();

    return this.products().filter(product => {
      const matchesSearch =
        product.productName.toLowerCase().includes(search) ||
        product.sku.toLowerCase().includes(search) ||
        product.categoryName.toLowerCase().includes(search);

      const matchesCategory =
        !category || product.categoryId.toString() === category;

      return matchesSearch && matchesCategory;
    });
  });

  totalProducts = computed(() => this.products().length);

  activeProducts = computed(() =>
    this.products().filter(p => p.isActive).length
  );

  inactiveProducts = computed(() =>
    this.products().filter(p => !p.isActive).length
  );

  ngOnInit(): void {
    this.loadProducts();
    this.loadCategories();
  }

  loadProducts(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.productService.getAll()
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: data => this.products.set(data),
        error: error => this.errorMessage.set(this.getErrorMessage(error))
      });
  }

  loadCategories(): void {
    this.categoryService.getAll().subscribe({
      next: data => this.categories.set(data.filter(c => c.isActive)),
      error: error => this.errorMessage.set(this.getErrorMessage(error))
    });
  }

  openAddForm(): void {
    this.resetForm();
    this.isEditMode.set(false);
    this.showForm.set(true);
  }

  openEditForm(product: Product): void {
    this.resetForm();

    this.isEditMode.set(true);
    this.editingProductId.set(product.productId);

    this.productName = product.productName;
    this.sku = product.sku;
    this.description = product.description ?? '';
    this.purchasePrice = product.purchasePrice;
    this.sellingPrice = product.sellingPrice;
    this.reorderLevel = product.reorderLevel;
    this.categoryId = product.categoryId;
    this.isActive = product.isActive;

    this.showForm.set(true);
  }

  closeForm(): void {
    this.showForm.set(false);
    this.resetForm();
  }

  resetForm(): void {
    this.productName = '';
    this.sku = '';
    this.description = '';
    this.purchasePrice = null;
    this.sellingPrice = null;
    this.reorderLevel = 0;
    this.categoryId = null;
    this.isActive = true;
    this.editingProductId.set(null);
    this.errorMessage.set('');
  }

  saveProduct(): void {
    this.errorMessage.set('');
    this.successMessage.set('');

    if (
      !this.productName.trim() ||
      !this.sku.trim() ||
      this.purchasePrice === null ||
      this.sellingPrice === null ||
      this.reorderLevel === null ||
      this.categoryId === null
    ) {
      this.errorMessage.set('Please fill all required fields.');
      return;
    }

    if (this.purchasePrice <= 0 || this.sellingPrice <= 0) {
      this.errorMessage.set('Prices must be greater than zero.');
      return;
    }

    if (this.reorderLevel < 0) {
      this.errorMessage.set('Reorder level cannot be negative.');
      return;
    }

    const createData: CreateProduct = {
      productName: this.productName.trim(),
      sku: this.sku.trim(),
      description: this.description.trim() || null,
      purchasePrice: this.purchasePrice,
      sellingPrice: this.sellingPrice,
      reorderLevel: this.reorderLevel,
      categoryId: this.categoryId
    };

    this.isSaving.set(true);

    if (this.isEditMode()) {
      const id = this.editingProductId();

      if (id === null) {
        this.isSaving.set(false);
        return;
      }

      const updateData: UpdateProduct = {
        ...createData,
        isActive: this.isActive
      };

      this.productService.update(id, updateData)
        .pipe(finalize(() => this.isSaving.set(false)))
        .subscribe({
          next: () => {
            this.successMessage.set('Product updated successfully.');
            this.showForm.set(false);
            this.loadProducts();
          },
          error: error => this.errorMessage.set(this.getErrorMessage(error))
        });

    } else {
      this.productService.create(createData)
        .pipe(finalize(() => this.isSaving.set(false)))
        .subscribe({
          next: () => {
            this.successMessage.set('Product added successfully.');
            this.showForm.set(false);
            this.loadProducts();
          },
          error: error => this.errorMessage.set(this.getErrorMessage(error))
        });
    }
  }

  deleteProduct(product: Product): void {
    const confirmed = window.confirm(
      `Are you sure you want to delete "${product.productName}"?`
    );

    if (!confirmed) return;

    this.errorMessage.set('');
    this.successMessage.set('');

    this.productService.delete(product.productId).subscribe({
      next: response => {
        this.successMessage.set(response.message);
        this.loadProducts();
      },
      error: error => this.errorMessage.set(this.getErrorMessage(error))
    });
  }

  private getErrorMessage(error: HttpErrorResponse): string {
    return error.error?.message ||
      error.error?.title ||
      'Something went wrong. Please try again.';
  }
}