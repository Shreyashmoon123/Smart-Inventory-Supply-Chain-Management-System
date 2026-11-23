export interface LowStockProduct {
  productId: number;
  productName: string;
  sku: string;
  warehouseName: string;
  quantity: number;
  reorderLevel: number;
}