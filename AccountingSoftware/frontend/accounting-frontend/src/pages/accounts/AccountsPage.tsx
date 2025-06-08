import React, { useState, useEffect } from 'react';
import { Account, AccountType, CreateAccountDto, UpdateAccountDto } from '../../types';
import { accountService } from '../../services/api';
import AccountModal from '../../components/modals/AccountModal';
import DeleteConfirmationModal from '../../components/modals/DeleteConfirmationModal';

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
      <button className="btn btn-sm btn-outline-danger ms-2" onClick={loadAccounts}>
        Try Again
      </button>
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
              onClick={handleAddAccount}
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
                          <span className={`badge ${account.isActive ? 'bg-success' : 'bg-secondary'}`}>
                            {account.isActive ? 'Active' : 'Inactive'}
                          </span>
                        </td>
                        <td>
                          <div className="btn-group" role="group">
                            <button 
                              className="btn btn-outline-primary btn-sm" 
                              onClick={() => handleEditAccount(account)}
                              title="Edit Account"
                            >
                              <i className="bi bi-pencil"></i>
                            </button>
                            <button 
                              className="btn btn-outline-danger btn-sm" 
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
                onClick={handleAddAccount}
              >
                <i className="bi bi-plus-circle me-2"></i>
                Create Account
              </button>
            </div>
          )}
        </div>
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