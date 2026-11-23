
export interface CreateProduct {
  productName: string;
  sku: string;
  description: string | null;
  purchasePrice: number;
  sellingPrice: number;
  reorderLevel: number;
  categoryId: number;
}
