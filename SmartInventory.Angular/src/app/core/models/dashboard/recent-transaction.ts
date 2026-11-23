export interface RecentTransaction {
  stockTransactionId: number;
  productName: string;
  warehouseName: string;
  transactionType: string;
  quantity: number;
  notes: string | null;
  transactionDate: string;
}