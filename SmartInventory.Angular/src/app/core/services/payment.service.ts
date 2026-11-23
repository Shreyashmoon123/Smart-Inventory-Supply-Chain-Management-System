import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

import {
  Payment,
  CreatePayment,
  PaymentSummary,
  UpdatePaymentStatus
} from '../models/payments/payment';

@Injectable({
  providedIn: 'root'
})
export class PaymentService {

  private http = inject(HttpClient);

  private apiUrl = environment.apiBaseUrl;

  getAll(): Observable<Payment[]> {
    return this.http.get<Payment[]>(
      `${this.apiUrl}api/Payments`
    );
  }

  getById(id: number): Observable<Payment> {
    return this.http.get<Payment>(
      `${this.apiUrl}api/Payments/${id}`
    );
  }

  getBySalesOrderId(id: number): Observable<Payment[]> {
    return this.http.get<Payment[]>(
      `${this.apiUrl}api/Payments/sales-order/${id}`
    );
  }

  getSummary(id: number): Observable<PaymentSummary> {
    return this.http.get<PaymentSummary>(
      `${this.apiUrl}api/Payments/sales-order/${id}/summary`
    );
  }

  create(data: CreatePayment): Observable<Payment> {
    return this.http.post<Payment>(
      `${this.apiUrl}api/Payments`,
      data
    );
  }

  updateStatus(
    id: number,
    data: UpdatePaymentStatus
  ): Observable<Payment> {
    return this.http.patch<Payment>(
      `${this.apiUrl}api/Payments/${id}/status`,
      data
    );
  }
}