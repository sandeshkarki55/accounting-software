// Invoice-related types
export enum InvoiceStatus {
  Draft = 0,
  Sent = 1,
  Paid = 2,
  Overdue = 3,
  Cancelled = 4
}

export interface InvoiceItem {
  id?: number;
  invoiceId?: number;
  description: string;
  quantity: number;
  unitPrice: number;
  amount: number;
  sortOrder: number;
}

export interface CreateInvoiceItemDto {
  description: string;
  quantity: number;
  unitPrice: number;
  sortOrder: number;
}

export interface Invoice {
  id: number;
  invoiceNumber: string;
  invoiceDate: string;
  dueDate: string;
  customerId: number;
  customerName: string;
  companyInfoId?: number;
  companyName?: string;
  description?: string;
  subTotal: number;
  taxRate: number;
  taxAmount: number;
  discountAmount: number;
  totalAmount: number;
  status: InvoiceStatus;
  statusName: string;
  notes?: string;
  terms?: string;
  paidDate?: string;
  paymentReference?: string;
  items: InvoiceItem[];
}

export interface CreateInvoiceDto {
  invoiceNumber: string;
  invoiceDate: string;
  dueDate: string;
  customerId: number;
  companyInfoId?: number;
  description?: string;
  taxRate: number;
  discountAmount: number;
  notes?: string;
  terms?: string;
  items: CreateInvoiceItemDto[];
}