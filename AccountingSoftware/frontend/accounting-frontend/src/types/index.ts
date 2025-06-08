// Types for the accounting system
export interface Account {
  id: number;
  accountCode: string;
  accountName: string;
  accountType: AccountType;
  balance: number;
  isActive: boolean;
  description: string;
  parentAccountId?: number;
  parentAccountName?: string;
}

export interface CreateAccountDto {
  accountCode: string;
  accountName: string;
  accountType: AccountType;
  description: string;
  parentAccountId?: number;
}

export interface UpdateAccountDto {
  accountName: string;
  description: string;
  isActive: boolean;
}

export enum AccountType {
  Asset = 0,
  Liability = 1,
  Equity = 2,
  Revenue = 3,
  Expense = 4
}

export interface JournalEntry {
  id: number;
  entryNumber: string;
  transactionDate: string;
  description: string;
  reference: string;
  totalAmount: number;
  isPosted: boolean;
  lines: JournalEntryLine[];
}

export interface JournalEntryLine {
  id: number;
  accountId: number;
  accountCode: string;
  accountName: string;
  debitAmount: number;
  creditAmount: number;
  description: string;
}

// Customer types
export interface Customer {
  id: number;
  customerCode: string;
  companyName: string;
  contactPersonName?: string;
  email?: string;
  phone?: string;
  address?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
  isActive: boolean;
  notes?: string;
}

export interface CreateCustomerDto {
  customerCode: string;
  companyName: string;
  contactPersonName?: string;
  email?: string;
  phone?: string;
  address?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
  notes?: string;
}

export interface UpdateCustomerDto {
  companyName: string;
  contactPersonName?: string;
  email?: string;
  phone?: string;
  address?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
  isActive: boolean;
  notes?: string;
}

// Company Info types
export interface CompanyInfo {
  id: number;
  companyName: string;
  legalName?: string;
  taxNumber?: string;
  registrationNumber?: string;
  email?: string;
  phone?: string;
  website?: string;
  address?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
  logoUrl?: string;
  bankName?: string;
  bankAccountNumber?: string;
  bankRoutingNumber?: string;
  currency: string;
  isDefault: boolean;
}

export interface CreateCompanyInfoDto {
  companyName: string;
  legalName?: string;
  taxNumber?: string;
  registrationNumber?: string;
  email?: string;
  phone?: string;
  website?: string;
  address?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
  logoUrl?: string;
  bankName?: string;
  bankAccountNumber?: string;
  bankRoutingNumber?: string;
  currency: string;
  isDefault: boolean;
}

// Invoice types
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