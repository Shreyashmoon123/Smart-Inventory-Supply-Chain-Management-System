export interface UpdateSupplier {
  supplierName: string;
  email: string | null;
  phone: string | null;
  address: string | null;
  isActive: boolean;
}