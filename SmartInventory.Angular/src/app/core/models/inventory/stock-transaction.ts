export interface StockTransaction {
  stockTransactionId: number;
  productId: number;
  productName: string;
  warehouseId: number;
  warehouseName: string;
  userId: number | null;
  transactionType: string | number;
  quantity: number;
  referenceType: string | null;
  referenceId: number | null;
  notes: string | null;
  transactionDate: string;
}