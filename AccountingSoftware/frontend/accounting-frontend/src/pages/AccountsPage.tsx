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

  if (loading) return (
    <div className="d-flex justify-content-center align-items-center" style={{minHeight: '200px'}}>
      <div className="spinner-border text-primary" role="status">
        <span className="visually-hidden">Loading accounts...</span>
      </div>
    </div>
  );
  
  if (error) return (
    <div className="alert alert-danger" role="alert">
      <strong>Error:</strong> {error}
    </div>
  );

  return (
    <div className="container">
      <div className="row">
        <div className="col-12">
          <h1 className="mb-4 text-dark">Chart of Accounts</h1>
          
          <div className="mb-4">
            <button 
              className="btn btn-primary" 
              onClick={() => console.log('Add account')}
            >
              <i className="bi bi-plus-circle me-2"></i>
              Add New Account
            </button>
          </div>

          <div className="card shadow-sm">
            <div className="card-body p-0">
              <div className="table-responsive">
                <table className="table table-hover mb-0">
                  <thead className="table-dark">
                    <tr>
                      <th scope="col">Account Code</th>
                      <th scope="col">Account Name</th>
                      <th scope="col">Type</th>
                      <th scope="col">Balance</th>
                      <th scope="col">Status</th>
                      <th scope="col">Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {accounts.map((account) => (
                      <tr key={account.id}>
                        <td className="fw-bold">{account.accountCode}</td>
                        <td>{account.accountName}</td>
                        <td>
                          <span className={`badge ${
                            account.accountType === AccountType.Asset ? 'bg-primary' :
                            account.accountType === AccountType.Liability ? 'bg-warning' :
                            account.accountType === AccountType.Equity ? 'bg-info' :
                            account.accountType === AccountType.Revenue ? 'bg-success' :
                            'bg-secondary'
                          }`}>
                            {getAccountTypeName(account.accountType)}
                          </span>
                        </td>
                        <td className="fw-bold">${account.balance.toFixed(2)}</td>
                        <td>
                          <span className={`badge ${account.isActive ? 'bg-success' : 'bg-danger'}`}>
                            {account.isActive ? 'Active' : 'Inactive'}
                          </span>
                        </td>
                        <td>
                          <div className="btn-group" role="group">
                            <button 
                              className="btn btn-outline-primary btn-sm" 
                              onClick={() => console.log('Edit', account.id)}
                              title="Edit Account"
                            >
                              <i className="bi bi-pencil"></i>
                            </button>
                            <button 
                              className="btn btn-outline-danger btn-sm" 
                              onClick={() => console.log('Delete', account.id)}
                              title="Delete Account"
                            >
                              <i className="bi bi-trash"></i>
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          </div>

          {accounts.length === 0 && (
            <div className="text-center py-5">
              <div className="mb-3">
                <i className="bi bi-folder-x display-1 text-muted"></i>
              </div>
              <h5 className="text-muted">No accounts found</h5>
              <p className="text-muted">Create your first account to get started.</p>
              <button 
                className="btn btn-primary" 
                onClick={() => console.log('Add account')}
              >
                <i className="bi bi-plus-circle me-2"></i>
                Create Account
              </button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default AccountsPage;