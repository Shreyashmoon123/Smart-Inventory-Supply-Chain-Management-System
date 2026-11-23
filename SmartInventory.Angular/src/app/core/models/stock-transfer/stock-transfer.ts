
export type StockTransferStatus =
  | 'Pending'
  | 'Approved'
  | 'InTransit'
  | 'Completed'
  | 'Cancelled';

export interface StockTransferItem {
  stockTransferItemId: number;
  productId: number;
  productName: string;
  sku: string;
  quantity: number;
}

export interface StockTransfer {
  stockTransferId: number;
  sourceWarehouseId: number;
  sourceWarehouseName: string;
  destinationWarehouseId: number;
  destinationWarehouseName: string;
  transferDate: string;
  status: StockTransferStatus;
  notes?: string | null;
  items: StockTransferItem[];
}

export interface CreateStockTransferItem {
  productId: number;
  quantity: number;
}

export interface CreateStockTransfer {
  sourceWarehouseId: number;
  destinationWarehouseId: number;
  notes?: string | null;
  items: CreateStockTransferItem[];
}
