import React, { useState, useEffect } from 'react';
import { Account, AccountType, CreateAccountDto, UpdateAccountDto } from '../../types';
import { accountService } from '../../services/api';
import AccountModal from '../../components/modals/AccountModal';
import DeleteConfirmationModal from '../../components/modals/DeleteConfirmationModal';
import './AccountsPage.scss';

const AccountsPage: React.FC = () => {
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showAccountModal, setShowAccountModal] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [selectedAccount, setSelectedAccount] = useState<Account | undefined>();
  const [accountToDelete, setAccountToDelete] = useState<Account | undefined>();
  const [deleteLoading, setDeleteLoading] = useState(false);

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

  const handleAddAccount = () => {
    setSelectedAccount(undefined);
    setShowAccountModal(true);
  };

  const handleEditAccount = (account: Account) => {
    setSelectedAccount(account);
    setShowAccountModal(true);
  };

  const handleDeleteAccount = (account: Account) => {
    setAccountToDelete(account);
    setShowDeleteModal(true);
  };

  const handleSaveAccount = async (accountData: CreateAccountDto | UpdateAccountDto) => {
    try {
      if (selectedAccount) {
        await accountService.updateAccount(selectedAccount.id, accountData as UpdateAccountDto);
      } else {
        await accountService.createAccount(accountData as CreateAccountDto);
      }
      await loadAccounts();
      setShowAccountModal(false);
    } catch (error) {
      console.error('Error saving account:', error);
      throw error;
    }
  };

  const handleConfirmDelete = async () => {
    if (!accountToDelete) return;

    try {
      setDeleteLoading(true);
      await accountService.deleteAccount(accountToDelete.id);
      await loadAccounts();
      setShowDeleteModal(false);
      setAccountToDelete(undefined);
    } catch (error) {
      console.error('Error deleting account:', error);
      setError('Failed to delete account');
    } finally {
      setDeleteLoading(false);
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

  const getAccountTypeColorClass = (type: AccountType): string => {
    switch (type) {
      case AccountType.Asset: return 'bg-vibrant-primary';
      case AccountType.Liability: return 'bg-vibrant-warning';
      case AccountType.Equity: return 'text-bg-info';
      case AccountType.Revenue: return 'bg-vibrant-success';
      case AccountType.Expense: return 'text-bg-secondary';
      default: return 'text-bg-dark';
    }
  };

  if (loading) return (
    <div className="accounts-page-loading">
      <div className="spinner-border text-vibrant-primary" role="status">
        <span className="visually-hidden">Loading accounts...</span>
      </div>
      <p className="mt-3 text-muted">Loading your chart of accounts...</p>
    </div>
  );
  
  if (error) return (
    <div className="alert alert-danger" role="alert">
      <div className="d-flex align-items-center">
        <i className="bi bi-exclamation-triangle-fill me-2"></i>
        <div>
          <strong>Error:</strong> {error}
        </div>
      </div>
      <button className="btn btn-sm btn-outline-danger mt-2" onClick={loadAccounts}>
        <i className="bi bi-arrow-clockwise me-1"></i>
        Try Again
      </button>
    </div>
  );

  return (
    <div className="accounts-page">
      <div className="page-header">
        <div className="page-title-section">
          <h1 className="page-title">
            <i className="bi bi-list-ul me-3"></i>
            Chart of Accounts
          </h1>
          <p className="page-subtitle">Manage your accounting structure and track balances</p>
        </div>
        
        <div className="page-actions">
          <button 
            className="btn btn-primary add-account-btn" 
            onClick={handleAddAccount}
          >
            <i className="bi bi-plus-circle me-2"></i>
            Add New Account
          </button>
        </div>
      </div>

      <div className="accounts-content">
        <div className="accounts-table-card">
          <div className="table-responsive">
            <table className="table table-hover accounts-table">
              <thead>
                <tr>
                  <th scope="col">
                    <i className="bi bi-hash me-1"></i>
                    Account Code
                  </th>
                  <th scope="col">
                    <i className="bi bi-tag me-1"></i>
                    Account Name
                  </th>
                  <th scope="col">
                    <i className="bi bi-collection me-1"></i>
                    Type
                  </th>
                  <th scope="col">
                    <i className="bi bi-currency-dollar me-1"></i>
                    Balance
                  </th>
                  <th scope="col">
                    <i className="bi bi-toggle-on me-1"></i>
                    Status
                  </th>
                  <th scope="col">
                    <i className="bi bi-gear me-1"></i>
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody>
                {accounts.map((account) => (
                  <tr key={account.id} className="account-row">
                    <td className="account-code">{account.accountCode}</td>
                    <td className="account-name">{account.accountName}</td>
                    <td>
                      <span className={`badge account-type-badge ${getAccountTypeColorClass(account.accountType)}`}>
                        {getAccountTypeName(account.accountType)}
                      </span>
                    </td>
                    <td className="account-balance">
                      ${account.balance.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                    </td>
                    <td>
                      <span className={`badge status-badge ${account.isActive ? 'bg-vibrant-success' : 'bg-vibrant-danger'}`}>
                        <i className={`bi ${account.isActive ? 'bi-check-circle' : 'bi-x-circle'} me-1`}></i>
                        {account.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                    <td>
                      <div className="action-buttons">
                        <button 
                          className="btn btn-outline-primary btn-sm action-btn" 
                          onClick={() => handleEditAccount(account)}
                          title="Edit Account"
                        >
                          <i className="bi bi-pencil"></i>
                        </button>
                        <button 
                          className="btn btn-outline-danger btn-sm action-btn" 
                          onClick={() => handleDeleteAccount(account)}
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

        {accounts.length === 0 && (
          <div className="empty-state">
            <div className="empty-state-icon">
              <i className="bi bi-folder-x"></i>
            </div>
            <h5 className="empty-state-title">No accounts found</h5>
            <p className="empty-state-description">Create your first account to get started with your chart of accounts.</p>
            <button 
              className="btn btn-primary" 
              onClick={handleAddAccount}
            >
              <i className="bi bi-plus-circle me-2"></i>
              Create Your First Account
            </button>
          </div>
        )}
      </div>

      {/* Account Modal for Add/Edit */}
      <AccountModal
        show={showAccountModal}
        onHide={() => setShowAccountModal(false)}
        onSave={handleSaveAccount}
        account={selectedAccount}
        accounts={accounts}
      />

      {/* Delete Confirmation Modal */}
      <DeleteConfirmationModal
        show={showDeleteModal}
        onHide={() => setShowDeleteModal(false)}
        onConfirm={handleConfirmDelete}
        accountName={accountToDelete?.accountName || ''}
        loading={deleteLoading}
      />
    </div>
  );
};

export default AccountsPage;