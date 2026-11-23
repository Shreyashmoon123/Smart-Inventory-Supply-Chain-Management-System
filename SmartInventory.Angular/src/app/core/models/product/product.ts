
export interface Product {
  productId: number;
  productName: string;
  sku: string;
  description: string | null;
  purchasePrice: number;
  sellingPrice: number;
  reorderLevel: number;
  categoryId: number;
  categoryName: string;
  isActive: boolean;
}
