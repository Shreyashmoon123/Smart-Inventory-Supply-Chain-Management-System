export interface Customer {
  customerId: number;
  customerName: string;
  email: string | null;
  phone: string | null;
  address: string | null;
  createdAt: string;
}