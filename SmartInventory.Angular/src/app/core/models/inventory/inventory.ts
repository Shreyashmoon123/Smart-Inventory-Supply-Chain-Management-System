export interface Inventory {
  inventoryId: number;
  productId: number;
  productName: string;
  sku: string;
  warehouseId: number;
  warehouseName: string;
  quantity: number;
  reorderLevel: number;
  isLowStock: boolean;
  lastUpdated: string;
}