export interface ReceivePurchaseOrderItem {
  purchaseOrderItemId: number;
  receivedQuantity: number;
}

export interface ReceivePurchaseOrder {
  items: ReceivePurchaseOrderItem[];
  notes: string | null;
}