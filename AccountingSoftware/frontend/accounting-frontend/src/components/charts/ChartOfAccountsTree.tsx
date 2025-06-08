import React from 'react';
import { Account, AccountType } from '../../types';
import './ChartOfAccountsTree.css';

interface ChartOfAccountsTreeProps {
  accounts: Account[];
  onEditAccount: (account: Account) => void;
  onDeleteAccount: (account: Account) => void;
}

const ChartOfAccountsTree: React.FC<ChartOfAccountsTreeProps> = ({
  accounts,
  onEditAccount,
  onDeleteAccount
}) => {
  const getAccountTypeName = (type: AccountType): string => {
    switch (type) {
      case AccountType.Asset: return 'Asset';
      case AccountType.Liability: return 'Liability';
      case AccountType.Equity: return 'Equity';
      case AccountType.Revenue: return 'Revenue';
      case AccountType.Expense: return 'Expense';
      default: return 'Unknown';
    }
  };

  const getIndentationStyle = (level: number) => ({
    paddingLeft: `${level * 2}rem`,
    position: 'relative' as const,
  });

  const getTreeIndicator = (level: number, hasChildren: boolean) => {
    if (level === 0) return null;
    
    return (
      <span 
        className="tree-indicator" 
        style={{ 
          position: 'absolute', 
          left: `${(level - 1) * 2 + 0.5}rem`,
          color: '#6c757d'
        }}
      >
        {hasChildren ? '├─ ' : '└─ '}
      </span>
    );
  };

  const groupAccountsByType = (accounts: Account[]) => {
    const grouped = accounts.reduce((acc, account) => {
      const typeName = getAccountTypeName(account.accountType);
      if (!acc[typeName]) {
        acc[typeName] = [];
      }
      acc[typeName].push(account);
      return acc;
    }, {} as Record<string, Account[]>);

    // Order by account type hierarchy
    const orderedTypes = ['Asset', 'Liability', 'Equity', 'Revenue', 'Expense'];
    const result: Record<string, Account[]> = {};
    
    orderedTypes.forEach(type => {
      if (grouped[type]) {
        result[type] = grouped[type];
      }
    });

    return result;
  };

  const groupedAccounts = groupAccountsByType(accounts);

  return (
    <div className="card shadow-sm chart-of-accounts-tree">
      <div className="card-header bg-primary text-white">
        <h5 className="mb-0">
          <i className="bi bi-diagram-3 me-2"></i>
          Chart of Accounts - Tree Structure
        </h5>
      </div>
      <div className="card-body p-0">
        <div className="table-responsive">
          <table className="table table-hover mb-0">
            <thead className="table-dark sticky-top">
              <tr>
                <th scope="col" style={{ width: '40%' }}>Account</th>
                <th scope="col" style={{ width: '15%' }}>Code</th>
                <th scope="col" style={{ width: '15%' }}>Type</th>
                <th scope="col" style={{ width: '15%' }}>Balance</th>
                <th scope="col" style={{ width: '10%' }}>Status</th>
                <th scope="col" style={{ width: '5%' }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {Object.entries(groupedAccounts).map(([typeName, typeAccounts]) => (
                <React.Fragment key={typeName}>
                  {/* Type Header Row */}
                  <tr className="table-secondary">
                    <td colSpan={6}>
                      <strong className="fs-6 text-uppercase">
                        <i className="bi bi-folder me-2"></i>
                        {typeName} Accounts
                      </strong>
                    </td>
                  </tr>
                  
                  {/* Account Rows */}
                  {typeAccounts.map((account, index) => {
                    const hasChildren = account.subAccounts && account.subAccounts.length > 0;
                    
                    return (
                      <tr 
                        key={account.id} 
                        className={`account-row level-${account.level || 0}`}
                        style={{ borderLeft: account.level && account.level > 0 ? '3px solid #e9ecef' : 'none' }}
                      >
                        <td style={getIndentationStyle(account.level || 0)}>
                          {getTreeIndicator(account.level || 0, !!hasChildren)}
                          <span className={`account-name ${(account.level || 0) > 0 ? 'text-muted' : 'fw-bold'}`}>
                            {account.accountName}
                          </span>
                          {account.parentAccountName && (
                            <small className="text-muted d-block" style={{ fontSize: '0.75rem' }}>
                              Parent: {account.parentAccountName}
                            </small>
                          )}
                        </td>
                        <td>
                          <code className="bg-light px-1 rounded">{account.accountCode}</code>
                        </td>
                        <td>
                          <span className={`badge ${
                            account.accountType === AccountType.Asset ? 'bg-primary' :
                            account.accountType === AccountType.Liability ? 'bg-warning text-dark' :
                            account.accountType === AccountType.Equity ? 'bg-info' :
                            account.accountType === AccountType.Revenue ? 'bg-success' :
                            'bg-secondary'
                          }`}>
                            {getAccountTypeName(account.accountType)}
                          </span>
                        </td>
                        <td className="text-end">
                          <span className={`fw-bold ${account.balance >= 0 ? 'text-success' : 'text-danger'}`}>
                            ${Math.abs(account.balance).toLocaleString('en-US', { 
                              minimumFractionDigits: 2, 
                              maximumFractionDigits: 2 
                            })}
                          </span>
                          {account.balance < 0 && (
                            <small className="text-danger d-block">CR</small>
                          )}
                        </td>
                        <td>
                          <span className={`badge ${account.isActive ? 'bg-success' : 'bg-secondary'}`}>
                            {account.isActive ? 'Active' : 'Inactive'}
                          </span>
                        </td>
                        <td>
                          <div className="btn-group" role="group">
                            <button 
                              className="btn btn-outline-primary btn-sm" 
                              onClick={() => onEditAccount(account)}
                              title="Edit Account"
                            >
                              <i className="bi bi-pencil"></i>
                            </button>
                            <button 
                              className="btn btn-outline-danger btn-sm" 
                              onClick={() => onDeleteAccount(account)}
                              title="Delete Account"
                            >
                              <i className="bi bi-trash"></i>
                            </button>
                          </div>
                        </td>
                      </tr>
                    );
                  })}
                </React.Fragment>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

export default ChartOfAccountsTree;