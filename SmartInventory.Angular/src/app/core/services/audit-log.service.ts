
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { AuditLog } from '../models/audit-logs/audit-log';

export interface AuditLogFilters {
  userId?: number;
  action?: string;
  entityName?: string;
  fromDate?: string;
  toDate?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuditLogService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiBaseUrl}api/AuditLogs`;

  getAll(filters?: AuditLogFilters): Observable<AuditLog[]> {
    let params = new HttpParams();

    if (filters?.userId !== undefined) {
      params = params.set('userId', filters.userId);
    }

    if (filters?.action) {
      params = params.set('action', filters.action);
    }

    if (filters?.entityName) {
      params = params.set('entityName', filters.entityName);
    }

    if (filters?.fromDate) {
      params = params.set('fromDate', filters.fromDate);
    }

    if (filters?.toDate) {
      params = params.set('toDate', filters.toDate);
    }

    return this.http.get<AuditLog[]>(this.apiUrl, { params });
  }

  getById(id: number): Observable<AuditLog> {
    return this.http.get<AuditLog>(`${this.apiUrl}/${id}`);
  }
}
