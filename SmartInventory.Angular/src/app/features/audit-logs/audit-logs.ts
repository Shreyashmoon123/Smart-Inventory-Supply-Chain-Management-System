
import {
  Component,
  OnInit,
  ChangeDetectorRef,
  inject
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';

import {
  AuditLogService,
  AuditLogFilters
} from '../../core/services/audit-log.service';

import { AuditLog } from '../../core/models/audit-logs/audit-log';

@Component({
  selector: 'app-audit-logs',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './audit-logs.html',
  styleUrl: './audit-logs.scss'
})
export class AuditLogs implements OnInit {
  private auditLogService = inject(AuditLogService);
  private cdr = inject(ChangeDetectorRef);

  auditLogs: AuditLog[] = [];
  selectedLog: AuditLog | null = null;

  loading = false;
  detailsLoading = false;
  errorMessage = '';

  userId = '';
  action = '';
  entityName = '';
  fromDate = '';
  toDate = '';
  searchTerm = '';

  showDetailsModal = false;

  ngOnInit(): void {
    this.loadAuditLogs();
  }

  loadAuditLogs(): void {
    if (
      this.fromDate &&
      this.toDate &&
      this.fromDate > this.toDate
    ) {
      this.errorMessage = 'From date cannot be greater than To date.';
      return;
    }

    if (
      this.userId.trim() &&
      (!Number.isInteger(Number(this.userId)) ||
        Number(this.userId) <= 0)
    ) {
      this.errorMessage = 'Please enter a valid User ID.';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    const filters: AuditLogFilters = {
      userId: this.userId.trim()
        ? Number(this.userId)
        : undefined,
      action: this.action.trim() || undefined,
      entityName: this.entityName.trim() || undefined,
      fromDate: this.fromDate || undefined,
      toDate: this.toDate || undefined
    };

    this.auditLogService.getAll(filters).subscribe({
      next: (data: AuditLog[]) => {
        this.auditLogs = data ?? [];
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (error: HttpErrorResponse) => {
        this.loading = false;
        this.errorMessage = this.getErrorMessage(error);
        this.cdr.markForCheck();
      }
    });
  }

  applyFilters(): void {
    this.loadAuditLogs();
  }

  resetFilters(): void {
    this.userId = '';
    this.action = '';
    this.entityName = '';
    this.fromDate = '';
    this.toDate = '';
    this.searchTerm = '';
    this.loadAuditLogs();
  }

  refresh(): void {
    this.loadAuditLogs();
  }

  viewDetails(id: number): void {
    this.detailsLoading = true;
    this.selectedLog = null;
    this.showDetailsModal = true;

    this.auditLogService.getById(id).subscribe({
      next: (data: AuditLog) => {
        this.selectedLog = data;
        this.detailsLoading = false;
        this.cdr.markForCheck();
      },
      error: (error: HttpErrorResponse) => {
        this.detailsLoading = false;
        this.errorMessage = this.getErrorMessage(error);
        this.showDetailsModal = false;
        this.cdr.markForCheck();
      }
    });
  }

  closeDetails(): void {
    this.showDetailsModal = false;
    this.selectedLog = null;
  }

  get filteredLogs(): AuditLog[] {
    const term = this.searchTerm.toLowerCase().trim();

    if (!term) {
      return this.auditLogs;
    }

    return this.auditLogs.filter(log =>
      [
        log.auditLogId,
        log.userId,
        log.userName,
        log.action,
        log.entityName,
        log.entityId,
        log.details
      ]
        .map(value => value ?? '')
        .join(' ')
        .toLowerCase()
        .includes(term)
    );
  }

  get totalLogs(): number {
    return this.auditLogs.length;
  }

  get uniqueUsers(): number {
    return new Set(
      this.auditLogs
        .map(log => log.userId)
        .filter((id): id is number => id !== null)
    ).size;
  }

  get todayLogs(): number {
    const today = new Date().toDateString();

    return this.auditLogs.filter(
      log => new Date(log.createdAt).toDateString() === today
    ).length;
  }

  get actionCount(): number {
    return new Set(
      this.auditLogs.map(log => log.action.toLowerCase())
    ).size;
  }

  private getErrorMessage(error: HttpErrorResponse): string {
    if (typeof error.error === 'string') {
      return error.error;
    }

    return error.error?.message ||
      error.error?.title ||
      'Unable to load audit logs. Please try again.';
  }
}
