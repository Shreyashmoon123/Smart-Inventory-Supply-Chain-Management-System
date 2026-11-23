export interface CreatePayment {
  salesOrderId: number;
  amount: number;
  paymentMethod: string;
  transactionReference: string | null;
}

export interface Payment {
  paymentId: number;
  salesOrderId: number;
  amount: number;
  paymentMethod: string;
  status: string;
  transactionReference: string | null;
  paymentDate: string;
}

export interface PaymentSummary {
  salesOrderId: number;
  orderAmount: number;
  totalPaid: number;
  remainingAmount: number;
  paymentStatus: string;
  payments: Payment[];
}

export interface UpdatePaymentStatus {
  status: string;
}