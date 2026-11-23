export interface AdjustStock {
  productId: number;
  warehouseId: number;
  newQuantity: number;
  notes: string | null;
}