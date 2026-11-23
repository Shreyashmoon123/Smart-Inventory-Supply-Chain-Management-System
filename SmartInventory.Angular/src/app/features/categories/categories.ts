
import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { finalize } from 'rxjs';

import { CategoryService } from '../../core/services/category.service';
import { AuthService } from '../../core/services/auth.service';

import type { Category } from '../../core/models/category/category';
import type { CreateCategory } from '../../core/models/category/create-category';
import type { UpdateCategory } from '../../core/models/category/update-category';

@Component({
  selector: 'app-categories',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './categories.html',
  styleUrl: './categories.scss'
})
export class Categories implements OnInit {
  private categoryService = inject(CategoryService);
  private authService = inject(AuthService);

  categories = signal<Category[]>([]);
  isLoading = signal(true);
  isSaving = signal(false);

  errorMessage = signal('');
  successMessage = signal('');

  searchText = '';
  showForm = false;
  isEditMode = false;
  selectedCategoryId: number | null = null;

  categoryName = '';
  description = '';
  isActive = true;

  get canManage(): boolean {
    return this.authService.hasRole(['Admin', 'Manager']);
  }

  get canDelete(): boolean {
    return this.authService.hasRole(['Admin']);
  }

  get filteredCategories(): Category[] {
    const search = this.searchText.trim().toLowerCase();

    if (!search) {
      return this.categories();
    }

    return this.categories().filter(category =>
      category.categoryName.toLowerCase().includes(search) ||
      (category.description ?? '').toLowerCase().includes(search)
    );
  }

  get activeCount(): number {
    return this.categories().filter(category => category.isActive).length;
  }

  get inactiveCount(): number {
    return this.categories().filter(category => !category.isActive).length;
  }

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.categoryService.getAll()
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: data => {
          this.categories.set(data);
        },
        error: error => {
          this.errorMessage.set(this.getErrorMessage(error));
        }
      });
  }

  openCreateForm(): void {
    this.resetForm();
    this.isEditMode = false;
    this.showForm = true;
  }

  openEditForm(category: Category): void {
    this.resetForm();

    this.isEditMode = true;
    this.showForm = true;

    this.selectedCategoryId = category.categoryId;
    this.categoryName = category.categoryName;
    this.description = category.description ?? '';
    this.isActive = category.isActive;
  }

  cancelForm(): void {
    this.showForm = false;
    this.resetForm();
  }

  saveCategory(): void {
    this.errorMessage.set('');
    this.successMessage.set('');

    const name = this.categoryName.trim();
    const details = this.description.trim();

    if (!name) {
      this.errorMessage.set('Category name is required.');
      return;
    }

    if (name.length > 100) {
      this.errorMessage.set('Category name cannot exceed 100 characters.');
      return;
    }

    if (details.length > 500) {
      this.errorMessage.set('Description cannot exceed 500 characters.');
      return;
    }

    this.isSaving.set(true);

    if (this.isEditMode && this.selectedCategoryId !== null) {
      const request: UpdateCategory = {
        categoryName: name,
        description: details || null,
        isActive: this.isActive
      };

      this.categoryService.update(this.selectedCategoryId, request)
        .pipe(finalize(() => this.isSaving.set(false)))
        .subscribe({
          next: () => {
            this.successMessage.set('Category updated successfully.');
            this.showForm = false;
            this.resetForm();
            this.loadCategories();
          },
          error: error => {
            this.errorMessage.set(this.getErrorMessage(error));
          }
        });

      return;
    }

    const request: CreateCategory = {
      categoryName: name,
      description: details || null
    };

    this.categoryService.create(request)
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: () => {
          this.successMessage.set('Category created successfully.');
          this.showForm = false;
          this.resetForm();
          this.loadCategories();
        },
        error: error => {
          this.errorMessage.set(this.getErrorMessage(error));
        }
      });
  }

  deleteCategory(category: Category): void {
    if (!this.canDelete) {
      return;
    }

    const confirmed = window.confirm(
      `Are you sure you want to delete "${category.categoryName}"?`
    );

    if (!confirmed) {
      return;
    }

    this.errorMessage.set('');
    this.successMessage.set('');

    this.categoryService.delete(category.categoryId)
      .subscribe({
        next: response => {
          this.successMessage.set(
            response.message || 'Category deleted successfully.'
          );
          this.loadCategories();
        },
        error: error => {
          this.errorMessage.set(this.getErrorMessage(error));
        }
      });
  }

  refreshCategories(): void {
    this.successMessage.set('');
    this.loadCategories();
  }

  private resetForm(): void {
    this.selectedCategoryId = null;
    this.categoryName = '';
    this.description = '';
    this.isActive = true;
  }

  private getErrorMessage(error: HttpErrorResponse): string {
    return error.error?.message ||
      error.error?.title ||
      'Something went wrong. Please try again.';
  }
}
