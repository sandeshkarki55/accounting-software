
import React, { useState, useEffect } from 'react';
import { Account, AccountType, CreateAccountDto, UpdateAccountDto, PaginationParams, SortingParams, AccountFilteringParams } from '../../types';
import { accountService } from '../../services/accountService';
import { usePageTitle } from '../../hooks/usePageTitle';
import usePagedData from '../../hooks/usePagedData';
import AccountModal from '../../components/modals/AccountModal';
import GenericDeleteConfirmationModal from '../../components/modals/GenericDeleteConfirmationModal';
import ChartOfAccountsTree from '../../components/charts/ChartOfAccountsTree';
import Pagination from '../../components/common/Pagination';

const AccountsPage: React.FC = () => {
  usePageTitle('Chart of Accounts');

  // Paged accounts for list view
  const {
    data: accounts,
    loading: loadingList,
    error: errorList,
    pagination,
    setPagination,
    sorting,
    setSorting,
    filtering,
    setFiltering,
    totalCount,
  } = usePagedData<Account, PaginationParams, SortingParams, AccountFilteringParams>({
    fetchData: accountService.getAccountsPaged,
    initialPagination: { pageNumber: 1, pageSize: 10 },
    initialSorting: { orderBy: 'accountCode', descending: false },
    initialFiltering: { searchTerm: '', accountType: undefined, isActive: undefined },
  });

  // Hierarchy accounts for tree view
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
    // Only load hierarchy accounts for tree view
    const loadHierarchy = async () => {
      try {
        setLoading(true);
        setError(null);
        const hierarchyData = await accountService.getAccountsHierarchy();
        setHierarchyAccounts(hierarchyData);
      } catch (err) {
        setError('Failed to load accounts');
        console.error('Error loading accounts:', err);
      } finally {
        setLoading(false);
      }
    };
    loadHierarchy();
  }, []);

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
      setPagination({ ...pagination }); // reload list view
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
      setShowDeleteModal(false);
      setAccountToDelete(undefined);
      setPagination({ ...pagination }); // reload list view
    } catch (error) {
      alert('Failed to delete account');
      console.error('Error deleting account:', error);
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


  // Sorting icon helper
  const renderSortIcon = (column: string) => {
    if (sorting.orderBy !== column) return null;
    return (
      <i className={`bi ms-1 bi-sort-${sorting.descending ? 'down' : 'up'}-alt`}></i>
    );
  };

  // Sorting handler
  const handleSort = (column: string) => {
    setSorting({
      orderBy: column,
      descending: sorting.orderBy === column ? !sorting.descending : false
    });
  };

  if (activeTab === 'list' && loadingList) return (
    <div className="d-flex justify-content-center align-items-center" style={{minHeight: '200px'}}>
      <div className="spinner-border text-primary" role="status">
        <span className="visually-hidden">Loading accounts...</span>
      </div>
    </div>
  );
  if (activeTab === 'list' && errorList) return (
    <div className="alert alert-danger" role="alert">
      <strong>Error:</strong> {errorList}
      <button className="btn btn-sm btn-outline-danger ms-2" onClick={() => setPagination({ ...pagination })}>
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
                {/* Search and Add Controls */}
                <div className="row mb-4">
                  <div className="col-md-6">
                    <div className="input-group">
                      <span className="input-group-text">
                        <i className="bi bi-search"></i>
                      </span>
                      <input
                        type="text"
                        className="form-control"
                        placeholder="Search accounts..."
                        value={filtering.searchTerm || ''}
                        onChange={e => setFiltering({ ...filtering, searchTerm: e.target.value })}
                      />
                    </div>
                  </div>
                  <div className="col-md-3">
                    <select
                      className="form-select"
                      value={filtering.accountType === undefined ? '' : filtering.accountType}
                      onChange={e => {
                        const val = e.target.value;
                        setFiltering({ ...filtering, accountType: val === '' ? undefined : Number(val) });
                      }}
                    >
                      <option value="">All Types</option>
                      <option value={AccountType.Asset}>Asset</option>
                      <option value={AccountType.Liability}>Liability</option>
                      <option value={AccountType.Equity}>Equity</option>
                      <option value={AccountType.Revenue}>Revenue</option>
                      <option value={AccountType.Expense}>Expense</option>
                    </select>
                  </div>
                  <div className="col-md-3">
                    <select
                      className="form-select"
                      value={filtering.isActive === undefined ? '' : filtering.isActive ? 'active' : 'inactive'}
                      onChange={e => {
                        const val = e.target.value;
                        setFiltering({ ...filtering, isActive: val === '' ? undefined : val === 'active' });
                      }}
                    >
                      <option value="">All Statuses</option>
                      <option value="active">Active Only</option>
                      <option value="inactive">Inactive Only</option>
                    </select>
                  </div>
                </div>
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
                            <th style={{ cursor: 'pointer' }} onClick={() => handleSort('accountCode')}>
                              Account Code
                              {renderSortIcon('accountCode')}
                            </th>
                            <th style={{ cursor: 'pointer' }} onClick={() => handleSort('accountName')}>
                              Account Name
                              {renderSortIcon('accountName')}
                            </th>
                            <th style={{ cursor: 'pointer' }} onClick={() => handleSort('accountType')}>
                              Type
                              {renderSortIcon('accountType')}
                            </th>
                            <th style={{ cursor: 'pointer' }} onClick={() => handleSort('parentAccountName')}>
                              Parent Account
                              {renderSortIcon('parentAccountName')}
                            </th>
                            <th style={{ cursor: 'pointer' }} onClick={() => handleSort('balance')}>
                              Balance
                              {renderSortIcon('balance')}
                            </th>
                            <th style={{ cursor: 'pointer' }} onClick={() => handleSort('isActive')}>
                              Status
                              {renderSortIcon('isActive')}
                            </th>
                            <th>Actions</th>
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

                {/* Results count */}
                {totalCount > 0 && (
                  <div className="mt-3 text-muted text-center">
                    Showing {(pagination.pageNumber - 1) * pagination.pageSize + 1} to {Math.min(pagination.pageNumber * pagination.pageSize, totalCount)} of {totalCount} accounts
                  </div>
                )}

                {/* Pagination Controls */}
                <Pagination
                  pageNumber={pagination.pageNumber}
                  pageSize={pagination.pageSize}
                  totalCount={totalCount}
                  onPageChange={page => setPagination({ ...pagination, pageNumber: page })}
                  ariaLabel="Account Pagination"
                />
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