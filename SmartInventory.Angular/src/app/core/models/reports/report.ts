
export interface SalesReport {
  salesOrderId: number;
  customerName: string;
  orderDate: string;
  status: string;
  totalAmount: number;
}

export interface PurchaseReport {
  purchaseOrderId: number;
  supplierName: string;
  warehouseName: string;
  orderDate: string;
  status: string;
  totalAmount: number;
}

export interface InventoryReport {
  productId: number;
  productName: string;
  sku: string;
  warehouseName: string;
  quantity: number;
  reorderLevel: number;
  isLowStock: boolean;
}

export interface StockTransactionReport {
  stockTransactionId: number;
  productName: string;
  warehouseName: string;
  transactionType: string;
  quantity: number;
  referenceType: string | null;
  referenceId: number | null;
  notes: string | null;
  transactionDate: string;
}

export type ReportType =
  | 'sales'
  | 'purchases'
  | 'inventory'
  | 'stock-transactions';
