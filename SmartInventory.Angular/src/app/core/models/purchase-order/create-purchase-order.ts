export interface CreatePurchaseOrderItem {
  productId: number;
  quantity: number;
  unitPrice: number;
}

export interface CreatePurchaseOrder {
  supplierId: number;
  warehouseId: number;
  expectedDeliveryDate: string | null;
  notes: string | null;
  items: CreatePurchaseOrderItem[];
}