import React, { useState, useEffect } from 'react';
import { Account, AccountType } from '../types';
import { accountService } from '../services/api';

const AccountsPage: React.FC = () => {
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadAccounts();
  }, []);

  const loadAccounts = async () => {
    try {
      setLoading(true);
      const data = await accountService.getAccounts();
      setAccounts(data);
      setError(null);
    } catch (err) {
      setError('Failed to load accounts');
      console.error('Error loading accounts:', err);
    } finally {
      setLoading(false);
    }
  };

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

  if (loading) return <div className="loading">Loading accounts...</div>;
  if (error) return <div className="error">Error: {error}</div>;

  return (
    <div className="accounts-page">
      <h1>Chart of Accounts</h1>
      
      <div className="accounts-controls">
        <button className="btn btn-primary" onClick={() => console.log('Add account')}>
          Add New Account
        </button>
      </div>

      <div className="accounts-table">
        <table>
          <thead>
            <tr>
              <th>Account Code</th>
              <th>Account Name</th>
              <th>Type</th>
              <th>Balance</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {accounts.map((account) => (
              <tr key={account.id}>
                <td>{account.accountCode}</td>
                <td>{account.accountName}</td>
                <td>{getAccountTypeName(account.accountType)}</td>
                <td>${account.balance.toFixed(2)}</td>
                <td>
                  <span className={`status ${account.isActive ? 'active' : 'inactive'}`}>
                    {account.isActive ? 'Active' : 'Inactive'}
                  </span>
                </td>
                <td>
                  <button className="btn btn-sm" onClick={() => console.log('Edit', account.id)}>
                    Edit
                  </button>
                  <button className="btn btn-sm btn-danger" onClick={() => console.log('Delete', account.id)}>
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {accounts.length === 0 && (
        <div className="empty-state">
          <p>No accounts found. Create your first account to get started.</p>
        </div>
      )}
    </div>
  );
};

export default AccountsPage;