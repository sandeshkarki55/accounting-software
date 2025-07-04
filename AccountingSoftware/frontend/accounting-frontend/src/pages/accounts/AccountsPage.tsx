import React, { useState, useEffect } from 'react';
import { Account, AccountType, CreateAccountDto, UpdateAccountDto } from '../../types';
import { accountService } from '../../services/api';
import { usePageTitle } from '../../hooks/usePageTitle';
import AccountModal from '../../components/modals/AccountModal';
import GenericDeleteConfirmationModal from '../../components/modals/GenericDeleteConfirmationModal';
import ChartOfAccountsTree from '../../components/charts/ChartOfAccountsTree';

const AccountsPage: React.FC = () => {
  usePageTitle('Chart of Accounts');

  const [accounts, setAccounts] = useState<Account[]>([]);
  const [hierarchyAccounts, setHierarchyAccounts] = useState<Account[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showAccountModal, setShowAccountModal] = useState(false);
  const [selectedAccount, setSelectedAccount] = useState<Account | undefined>();
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [accountToDelete, setAccountToDelete] = useState<Account | undefined>();
  const [deleteLoading, setDeleteLoading] = useState(false);
  const [activeTab, setActiveTab] = useState<'list' | 'tree'>('tree');

  useEffect(() => {
    loadAccounts();
  }, []);

  const loadAccounts = async () => {
    try {
      setLoading(true);
      setError(null);
      
      // Load both regular accounts and hierarchy accounts
      const [regularData, hierarchyData] = await Promise.all([
        accountService.getAccounts(),
        accountService.getAccountsHierarchy()
      ]);
      
      setAccounts(regularData);
      setHierarchyAccounts(hierarchyData);
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
    <div className="container-fluid">
      <div className="row">
        <div className="col-12">
          <div className="d-flex justify-content-between align-items-center mb-4">
            <h1 className="text-dark">Chart of Accounts</h1>
            <button 
              className="btn btn-primary" 
              onClick={handleAddAccount}
            >
              <i className="bi bi-plus-circle me-2"></i>
              Add Account
            </button>
          </div>

          {/* Tab Navigation */}
          <ul className="nav nav-tabs mb-4" id="accountsTabs" role="tablist">
            <li className="nav-item" role="presentation">
              <button 
                className={`nav-link ${activeTab === 'tree' ? 'active' : ''}`}
                onClick={() => setActiveTab('tree')}
                type="button"
                style={{ 
                  backgroundColor: activeTab === 'tree' ? '#fff' : 'transparent',
                  borderColor: activeTab === 'tree' ? '#dee2e6 #dee2e6 #fff' : '#dee2e6',
                  color: activeTab === 'tree' ? '#495057' : '#6c757d'
                }}
              >
                <i className="bi bi-diagram-3 me-2"></i>
                Tree View
              </button>
            </li>
            <li className="nav-item" role="presentation">
              <button 
                className={`nav-link ${activeTab === 'list' ? 'active' : ''}`}
                onClick={() => setActiveTab('list')}
                type="button"
                style={{ 
                  backgroundColor: activeTab === 'list' ? '#fff' : 'transparent',
                  borderColor: activeTab === 'list' ? '#dee2e6 #dee2e6 #fff' : '#dee2e6',
                  color: activeTab === 'list' ? '#495057' : '#6c757d'
                }}
              >
                <i className="bi bi-list me-2"></i>
                List View
              </button>
            </li>
          </ul>

          {/* Tab Content */}
          <div className="tab-content" id="accountsTabContent">
            {/* Tree View */}
            {activeTab === 'tree' && (
              <div className="tab-pane fade show active">
                <ChartOfAccountsTree
                  accounts={hierarchyAccounts}
                  onEditAccount={handleEditAccount}
                  onDeleteAccount={handleDeleteAccount}
                />
              </div>
            )}

            {/* List View */}
            {activeTab === 'list' && (
              <div className="tab-pane fade show active">
                <div className="card shadow-sm">
                  <div className="card-header bg-secondary text-white">
                    <h5 className="mb-0">
                      <i className="bi bi-list me-2"></i>
                      Chart of Accounts - List View
                    </h5>
                  </div>
                  <div className="card-body p-0">
                    <div className="table-responsive">
                      <table className="table table-hover mb-0">
                        <thead className="table-dark">
                          <tr>
                            <th scope="col">Account Code</th>
                            <th scope="col">Account Name</th>
                            <th scope="col">Type</th>
                            <th scope="col">Parent Account</th>
                            <th scope="col">Balance</th>
                            <th scope="col">Status</th>
                            <th scope="col">Actions</th>
                          </tr>
                        </thead>
                        <tbody>
                          {accounts.map((account) => (
                            <tr key={account.id}>
                              <td>
                                <code className="bg-light px-1 rounded">{account.accountCode}</code>
                              </td>
                              <td className="fw-bold">{account.accountName}</td>
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
                              <td>
                                {account.parentAccountName ? (
                                  <span className="text-muted">{account.parentAccountName}</span>
                                ) : (
                                  <span className="text-muted fst-italic">None</span>
                                )}
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
              </div>
            )}
          </div>

          {/* Account Modal */}
          <AccountModal
            show={showAccountModal}
            onHide={() => setShowAccountModal(false)}
            onSave={handleSaveAccount}
            account={selectedAccount}
            accounts={accounts}
          />

          {/* Delete Confirmation Modal */}
          <GenericDeleteConfirmationModal
            show={showDeleteModal}
            onHide={() => setShowDeleteModal(false)}
            onConfirm={handleConfirmDelete}
            itemName={accountToDelete?.accountName || ''}
            itemType="account"
            loading={deleteLoading}
            warningMessage="This will soft delete the account. Make sure this account has no transactions and sub-accounts before deleting."
          />
        </div>
      </div>
    </div>
  );
};

export default AccountsPage;