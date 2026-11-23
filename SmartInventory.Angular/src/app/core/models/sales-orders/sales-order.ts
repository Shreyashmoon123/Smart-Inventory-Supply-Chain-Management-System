export interface CreateSalesOrderItem {
  productId: number;
  quantity: number;
}

export interface CreateSalesOrder {
  customerId: number;
  notes: string | null;
  items: CreateSalesOrderItem[];
}

export interface SalesOrderItem {
  salesOrderItemId: number;
  productId: number;
  productName: string;
  sku: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface SalesOrder {
  salesOrderId: number;
  customerId: number;
  customerName: string;
  orderDate: string;
  status: string;
  totalAmount: number;
  notes: string | null;
  items: SalesOrderItem[];
}

export interface UpdateSalesOrderStatus {
  status: string;
}

export interface CustomerOption {
  customerId: number;
  customerName: string;
}

export interface ProductOption {
  productId: number;
  productName: string;
  sku: string;
  sellingPrice: number;
  isActive: boolean;
}