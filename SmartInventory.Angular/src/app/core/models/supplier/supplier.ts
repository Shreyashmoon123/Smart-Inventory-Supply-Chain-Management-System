export interface Supplier {
  supplierId: number;
  supplierName: string;
  email: string | null;
  phone: string | null;
  address: string | null;
  isActive: boolean;
}