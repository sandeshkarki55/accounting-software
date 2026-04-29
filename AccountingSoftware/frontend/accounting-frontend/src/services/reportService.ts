import apiClient from './apiClient';

export interface TrialBalanceLine {
  accountId: number;
  accountCode: string;
  accountName: string;
  accountType: string;
  debitBalance: number;
  creditBalance: number;
}

export interface TrialBalance {
  lines: TrialBalanceLine[];
  totalDebits: number;
  totalCredits: number;
  isBalanced: boolean;
}

export interface IncomeStatementSection {
  accountId: number;
  accountCode: string;
  accountName: string;
  amount: number;
}

export interface IncomeStatement {
  revenue: IncomeStatementSection[];
  totalRevenue: number;
  costOfGoodsSold: IncomeStatementSection[];
  totalCOGS: number;
  grossProfit: number;
  operatingExpenses: IncomeStatementSection[];
  totalOperatingExpenses: number;
  operatingIncome: number;
  otherIncome: IncomeStatementSection[];
  totalOtherIncome: number;
  otherExpenses: IncomeStatementSection[];
  totalOtherExpenses: number;
  netIncome: number;
}

export interface BalanceSheetSection {
  accountId: number;
  accountCode: string;
  accountName: string;
  balance: number;
}

export interface BalanceSheet {
  assets: BalanceSheetSection[];
  totalAssets: number;
  liabilities: BalanceSheetSection[];
  totalLiabilities: number;
  equity: BalanceSheetSection[];
  totalEquity: number;
  totalLiabilitiesAndEquity: number;
  isBalanced: boolean;
}

export interface GeneralLedgerLine {
  transactionDate: string;
  entryNumber: string;
  description: string;
  debitAmount: number;
  creditAmount: number;
  runningBalance: number;
}

export interface GeneralLedger {
  accountId: number;
  accountCode: string;
  accountName: string;
  accountType: string;
  lines: GeneralLedgerLine[];
  openingBalance: number;
  closingBalance: number;
}

export interface AgedReceivableInvoice {
  invoiceId: number;
  invoiceNumber: string;
  invoiceDate: string;
  dueDate: string;
  amount: number;
  daysOverdue: number;
}

export interface AgedReceivablesCustomer {
  customerId: number;
  customerName: string;
  customerCode: string;
  invoices: AgedReceivableInvoice[];
  currentAmount: number;
  days1To30Amount: number;
  days31To60Amount: number;
  days61To90Amount: number;
  daysOver90Amount: number;
  totalOutstanding: number;
}

export interface AgedReceivablesSummary {
  totalCurrent: number;
  totalDays1To30: number;
  totalDays31To60: number;
  totalDays61To90: number;
  totalDaysOver90: number;
  grandTotal: number;
}

export interface AgedReceivables {
  customers: AgedReceivablesCustomer[];
  summary: AgedReceivablesSummary;
}

export const reportService = {
  getTrialBalance: async (asOfDate?: string): Promise<TrialBalance> => {
    const response = await apiClient.get<TrialBalance>('/reports/trial-balance', { params: { asOfDate } });
    return response.data;
  },

  getIncomeStatement: async (startDate: string, endDate: string): Promise<IncomeStatement> => {
    const response = await apiClient.get<IncomeStatement>('/reports/income-statement', { params: { startDate, endDate } });
    return response.data;
  },

  getBalanceSheet: async (asOfDate?: string): Promise<BalanceSheet> => {
    const response = await apiClient.get<BalanceSheet>('/reports/balance-sheet', { params: { asOfDate } });
    return response.data;
  },

  getGeneralLedger: async (accountId: number, startDate: string, endDate: string): Promise<GeneralLedger> => {
    const response = await apiClient.get<GeneralLedger>('/reports/general-ledger', { params: { accountId, startDate, endDate } });
    return response.data;
  },

  getAgedReceivables: async (asOfDate?: string): Promise<AgedReceivables> => {
    const response = await apiClient.get<AgedReceivables>('/reports/aged-receivables', { params: { asOfDate } });
    return response.data;
  },
};
