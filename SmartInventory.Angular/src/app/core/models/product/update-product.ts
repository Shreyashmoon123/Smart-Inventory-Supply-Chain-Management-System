
export interface UpdateProduct {
  productName: string;
  sku: string;
  description: string | null;
  purchasePrice: number;
  sellingPrice: number;
  reorderLevel: number;
  categoryId: number;
  isActive: boolean;
}
