export interface StockIn {
  productId: number;
  warehouseId: number;
  quantity: number;
  referenceType: string | null;
  referenceId: number | null;
  notes: string | null;
}