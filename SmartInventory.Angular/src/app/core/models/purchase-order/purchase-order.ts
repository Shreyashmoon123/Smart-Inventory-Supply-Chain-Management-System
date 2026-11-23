export type PurchaseOrderStatus =
  | number
  | 'Pending'
  | 'Approved'
  | 'PartiallyReceived'
  | 'Received'
  | 'Cancelled';

export interface PurchaseOrderItem {
  purchaseOrderItemId: number;
  productId: number;
  productName: string;
  sku: string;
  quantity: number;
  receivedQuantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface PurchaseOrder {
  purchaseOrderId: number;
  supplierId: number;
  supplierName: string;
  warehouseId: number;
  warehouseName: string;
  orderDate: string;
  expectedDeliveryDate: string | null;
  receivedDate: string | null;
  status: PurchaseOrderStatus;
  totalAmount: number;
  notes: string | null;
  items: PurchaseOrderItem[];
}