// Account-related types
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
  level?: number; // For tree structure display
  subAccounts?: Account[]; // For hierarchical display
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

// Journal Entry types
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

export interface CreateJournalEntryLineDto {
  accountId: number;
  debitAmount: number;
  creditAmount: number;
  description: string;
}

export interface CreateJournalEntryDto {
  transactionDate: string;
  description: string;
  reference: string;
  lines: CreateJournalEntryLineDto[];
}

export interface UpdateJournalEntryDto {
  transactionDate: string;
  description: string;
  reference: string;
  lines: CreateJournalEntryLineDto[];
}