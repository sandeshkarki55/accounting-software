import { AccountType } from '../types';

export const accountTypeColor = (t: AccountType): string =>
  ({
    [AccountType.Asset]: 'blue',
    [AccountType.Liability]: 'red',
    [AccountType.Equity]: 'grape',
    [AccountType.Revenue]: 'green',
    [AccountType.Expense]: 'orange',
  }[t] ?? 'gray');

export const accountTypeName = (t: AccountType): string =>
  ({
    [AccountType.Asset]: 'Asset',
    [AccountType.Liability]: 'Liability',
    [AccountType.Equity]: 'Equity',
    [AccountType.Revenue]: 'Revenue',
    [AccountType.Expense]: 'Expense',
  }[t] ?? 'Unknown');

export const invoiceStatusColor = (status: string): string => {
  switch (status) {
    case 'Paid': return 'green';
    case 'Unpaid':
    case 'Sent': return 'yellow';
    case 'Overdue': return 'red';
    case 'Draft':
    case 'Cancelled': return 'gray';
    default: return 'gray';
  }
};

export const activeStatusColor = (isActive: boolean): string =>
  isActive ? 'green' : 'gray';
