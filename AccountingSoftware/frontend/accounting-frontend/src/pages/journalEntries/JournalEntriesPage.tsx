import React, { useState, useEffect } from 'react';
import Pagination from '../../components/common/Pagination';
import SortableTableHeader, { SortableColumn } from '../../components/common/SortableTableHeader';
import { JournalEntry, JournalEntryLine, CreateJournalEntryDto, UpdateJournalEntryDto, Account, PaginationParams, SortingParams, JournalEntryFilteringParams, PagedResult } from '../../types/index';
import { journalEntryService, accountService } from '../../services/api';
import { usePageTitle } from '../../hooks/usePageTitle';
import usePagedData from '../../hooks/usePagedData'; // Import the reusable hook
import JournalEntryModal from '../../components/modals/JournalEntryModal';
import PostJournalEntryModal from '../../components/modals/PostJournalEntryModal';
import GenericDeleteConfirmationModal from '../../components/modals/GenericDeleteConfirmationModal';

const JournalEntriesPage: React.FC = () => {
  usePageTitle('Journal Entries');

  // Use the usePagedData hook
  const { 
    data: journalEntries, 
    loading, 
    error, 
    pagination, 
    setPagination, 
    sorting, 
    setSorting, 
    filtering, 
    setFiltering, 
    totalCount, 
    // loadData // loadData is now managed by the hook's useEffect
  } = usePagedData<JournalEntry, PaginationParams, SortingParams, JournalEntryFilteringParams>({
    fetchData: journalEntryService.getJournalEntries,
    initialPagination: { pageNumber: 1, pageSize: 10 },
    initialSorting: { orderBy: 'transactionDate', descending: true },
    initialFiltering: { searchTerm: '', statusFilter: 'all' },
  });

  // Keep accounts state separate as it's not paged
  const [accounts, setAccounts] = useState<Account[]>([]);
  // Keep expandedRows state separate as it's UI state
  const [expandedRows, setExpandedRows] = useState<Set<number>>(new Set());


  // Fetch accounts data on initial load
  useEffect(() => {
    const loadAccounts = async () => {
      try {
        const accountsData = await accountService.getAccounts();
        setAccounts(accountsData);
      } catch (err) {
        console.error('Error loading accounts:', err);
        // Optionally set an error state specifically for accounts if needed
      }
    };
    loadAccounts();
  }, []);

  // Modal states
  const [showJournalEntryModal, setShowJournalEntryModal] = useState(false);
  const [selectedJournalEntry, setSelectedJournalEntry] = useState<JournalEntry | undefined>();
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [journalEntryToDelete, setJournalEntryToDelete] = useState<JournalEntry | undefined>();
  const [deleteLoading, setDeleteLoading] = useState(false);
  const [showPostModal, setShowPostModal] = useState(false);
  const [journalEntryToPost, setJournalEntryToPost] = useState<JournalEntry | undefined>();
  const [postLoading, setPostLoading] = useState(false);

  const toggleRowExpansion = (id: number) => {
    const newExpandedRows = new Set(expandedRows);
    if (newExpandedRows.has(id)) {
      newExpandedRows.delete(id);
    } else {
      newExpandedRows.add(id);
    }
    setExpandedRows(newExpandedRows);
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-US', { 
      style: 'currency', 
      currency: 'USD' 
    }).format(amount);
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString();
  };

  const getStatusBadgeClass = (isPosted: boolean) => {
    return isPosted ? 'bg-success' : 'bg-warning text-dark';
  };

  // CRUD handlers
  const handleAddJournalEntry = () => {
    setSelectedJournalEntry(undefined);
    setShowJournalEntryModal(true);
  };

  const handleEditJournalEntry = (entry: JournalEntry) => {
    if (!entry.isPosted) {
      setSelectedJournalEntry(entry);
      setShowJournalEntryModal(true);
    }
  };

  const handleDeleteJournalEntry = (entry: JournalEntry) => {
    setJournalEntryToDelete(entry);
    setShowDeleteModal(true);
  };

  const handleSaveJournalEntry = async (journalEntryData: CreateJournalEntryDto | UpdateJournalEntryDto, isUpdate: boolean = false, entryId?: number) => {
    try {
      if (isUpdate && entryId) {
        await journalEntryService.updateJournalEntry(entryId, journalEntryData as UpdateJournalEntryDto);
      } else {
        await journalEntryService.createJournalEntry(journalEntryData as CreateJournalEntryDto);
      }
      // Changing state will trigger the hook to reload data
      setPagination({ ...pagination }); // Trigger reload by updating pagination state
    } catch (error) {
      console.error('Error saving journal entry:', error);
      throw error;
    }
  };

  const handleConfirmDelete = async () => {
    if (!journalEntryToDelete) return;

    try {
      setDeleteLoading(true);
      await journalEntryService.deleteJournalEntry(journalEntryToDelete.id);
      // Changing state will trigger the hook to reload data
      setPagination({ ...pagination }); // Trigger reload by updating pagination state
      setShowDeleteModal(false);
      setJournalEntryToDelete(undefined);
    } catch (error) {
      console.error('Error deleting journal entry:', error);
    } finally {
      setDeleteLoading(false);
    }
  };

  const handlePostJournalEntry = (entry: JournalEntry) => {
    setJournalEntryToPost(entry);
    setShowPostModal(true);
  };

  const handleConfirmPost = async () => {
    if (!journalEntryToPost) return;

    try {
      setPostLoading(true);
      await journalEntryService.postJournalEntry(journalEntryToPost.id);
      // Changing state will trigger the hook to reload data
      setPagination({ ...pagination }); // Trigger reload by updating pagination state
      setShowPostModal(false);
      setJournalEntryToPost(undefined);
    } catch (error) {
      console.error('Error posting journal entry:', error);
    } finally {
      setPostLoading(false);
    }
  };

  if (loading) return (
    <div className="d-flex justify-content-center align-items-center" style={{minHeight: '200px'}}>
      <div className="spinner-border text-primary" role="status">
        <span className="visually-hidden">Loading journal entries...</span>
      </div>
    </div>
  );
  
  if (error) return (
    <div className="alert alert-danger" role="alert">
      <strong>Error:</strong> {error}
      {/* The hook manages loading, so clicking Try Again should just trigger a reload via state change */}
      <button className="btn btn-sm btn-outline-danger ms-2" onClick={() => setPagination({ ...pagination })}>
        Try Again
      </button>
    </div>
  );

  return (
    <div className="container">
      <div className="row">
        <div className="col-12">
          <h1 className="mb-4 text-dark">Journal Entries</h1>
          
          {/* Search and Filter Controls */}
          <div className="row mb-4">
            <div className="col-md-6">
              <div className="input-group">
                <span className="input-group-text">
                  <i className="bi bi-search"></i>
                </span>
                <input
                  type="text"
                  className="form-control"
                  placeholder="Search entries, descriptions, or references..."
                  value={filtering.searchTerm}
                  onChange={(e) => setFiltering({ ...filtering, searchTerm: e.target.value })}
                />
              </div>
            </div>
            <div className="col-md-3">
              <select
                className="form-select"
                value={filtering.statusFilter}
                onChange={(e) => setFiltering({ ...filtering, statusFilter: e.target.value as 'all' | 'posted' | 'unposted' })}
              >
                <option value="all">All Entries</option>
                <option value="posted">Posted Only</option>
                <option value="unposted">Unposted Only</option>
              </select>
            </div>
            <div className="col-md-3">
              <button 
                className="btn btn-primary w-100" 
                onClick={() => handleAddJournalEntry()}
              >
                <i className="bi bi-plus-circle me-2"></i>
                Create Entry
              </button>
            </div>
          </div>

          <div className="card shadow-sm">
            <div className="card-body p-0">
              <div className="table-responsive">
                <table className="table table-hover mb-0">
                  <SortableTableHeader
                    columns={[
                      { key: 'expander', label: '', sortable: false, style: { width: '50px' } },
                      { key: 'entryNumber', label: 'Entry #', sortable: true },
                      { key: 'transactionDate', label: 'Date', sortable: true },
                      { key: 'description', label: 'Description', sortable: true },
                      { key: 'reference', label: 'Reference', sortable: true },
                      { key: 'totalAmount', label: 'Total', sortable: true },
                      { key: 'status', label: 'Status', sortable: false },
                      { key: 'actions', label: 'Actions', sortable: false },
                    ]}
                    sorting={{ orderBy: sorting.orderBy || '', descending: sorting.descending ?? false }}
                    onSort={(column) => setSorting({ orderBy: column, descending: sorting.orderBy === column ? !sorting.descending : false })}
                  />
                  <tbody>
                    {journalEntries.map((entry) => (
                      <React.Fragment key={entry.id}>
                        {/* Main row */}
                        <tr>
                          <td>
                            <button
                              className="btn btn-sm btn-link p-0"
                              onClick={() => toggleRowExpansion(entry.id)}
                              title={expandedRows.has(entry.id) ? "Collapse details" : "Expand details"}
                            >
                              <i className={`bi ${expandedRows.has(entry.id) ? 'bi-chevron-down' : 'bi-chevron-right'}`}></i>
                            </button>
                          </td>
                          <td className="fw-bold">{entry.entryNumber}</td>
                          <td>{formatDate(entry.transactionDate)}</td>
                          <td>{entry.description}</td>
                          <td>{entry.reference}</td>
                          <td className="fw-bold">{formatCurrency(entry.totalAmount)}</td>
                          <td>
                            <span className={`badge ${getStatusBadgeClass(entry.isPosted)}`}>
                              {entry.isPosted ? 'Posted' : 'Draft'}
                            </span>
                          </td>
                          <td>
                            <div className="btn-group" role="group">
                              <button
                                className="btn btn-sm btn-outline-primary"
                                onClick={() => toggleRowExpansion(entry.id)}
                                title="View Details"
                              >
                                <i className="bi bi-eye"></i>
                              </button>
                              {!entry.isPosted && (
                                <>
                                  <button
                                    className="btn btn-sm btn-outline-secondary"
                                    onClick={() => handleEditJournalEntry(entry)}
                                    title="Edit Entry"
                                  >
                                    <i className="bi bi-pencil"></i>
                                  </button>
                                  <button
                                    className="btn btn-sm btn-outline-success"
                                    onClick={() => handlePostJournalEntry(entry)}
                                    title="Post Entry"
                                  >
                                    <i className="bi bi-check-circle"></i>
                                  </button>
                                  <button
                                    className="btn btn-sm btn-outline-danger"
                                    onClick={() => handleDeleteJournalEntry(entry)}
                                    title="Delete Entry"
                                  >
                                    <i className="bi bi-trash"></i>
                                  </button>
                                </>
                              )}
                            </div>
                          </td>
                        </tr>
                        
                        {/* Expanded row with journal entry lines */}
                        {expandedRows.has(entry.id) && (
                          <tr>
                            <td colSpan={8} className="p-0">
                              <div className="bg-light border-top">
                                <div className="p-3">
                                  <h6 className="mb-3 text-muted">
                                    <i className="bi bi-list-ul me-2"></i>
                                    Journal Entry Lines
                                  </h6>
                                  <div className="table-responsive">
                                    <table className="table table-sm mb-0">
                                      <thead>
                                        <tr>
                                          <th>Account Code</th>
                                          <th>Account Name</th>
                                          <th>Description</th>
                                          <th className="text-end">Debit</th>
                                          <th className="text-end">Credit</th>
                                        </tr>
                                      </thead>
                                      <tbody>
                                        {entry.lines.map((line: JournalEntryLine) => (
                                          <tr key={line.id}>
                                            <td className="fw-bold">{line.accountCode}</td>
                                            <td>{line.accountName}</td>
                                            <td>{line.description}</td>
                                            <td className="text-end">
                                              {line.debitAmount > 0 ? formatCurrency(line.debitAmount) : '-'}
                                            </td>
                                            <td className="text-end">
                                              {line.creditAmount > 0 ? formatCurrency(line.creditAmount) : '-'}
                                            </td>
                                          </tr>
                                        ))}
                                      </tbody>
                                      <tfoot>
                                        <tr className="table-secondary">
                                          <td colSpan={3} className="fw-bold">Totals:</td>
                                          <td className="text-end fw-bold">
                                            {formatCurrency(entry.lines.reduce((sum: number, line: JournalEntryLine) => sum + line.debitAmount, 0))}
                                          </td>
                                          <td className="text-end fw-bold">
                                            {formatCurrency(entry.lines.reduce((sum: number, line: JournalEntryLine) => sum + line.creditAmount, 0))}
                                          </td>
                                        </tr>
                                      </tfoot>
                                    </table>
                                  </div>
                                </div>
                              </div>
                            </td>
                          </tr>
                        )}
                      </React.Fragment>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          </div>

          {journalEntries.length === 0 && !loading && (
            <div className="text-center py-5">
              <div className="mb-3">
                <i className="bi bi-journal-text display-1 text-muted"></i>
              </div>
              <h5 className="text-muted">
                {filtering.searchTerm || filtering.statusFilter !== 'all' ? 'No entries match your criteria' : 'No journal entries found'}
              </h5>
              <p className="text-muted">
                {filtering.searchTerm || filtering.statusFilter !== 'all'
                  ? 'Try adjusting your search or filter settings.'
                  : 'Journal entries are automatically created when invoices are generated or paid.'}
              </p>
              {(!filtering.searchTerm && filtering.statusFilter === 'all') && (
                <button 
                  className="btn btn-primary" 
                  onClick={() => handleAddJournalEntry()}
                >
                  <i className="bi bi-plus-circle me-2"></i>
                  Create Entry
                </button>
              )}
            </div>
          )}

          {/* Results count */}
          {totalCount > 0 && (
            <div className="mt-3 text-muted text-center">
              Showing {(pagination.pageNumber - 1) * pagination.pageSize + 1} to {Math.min(pagination.pageNumber * pagination.pageSize, totalCount)} of {totalCount} journal entries
            </div>
          )}

          {/* Pagination Controls */}
          <Pagination
            pageNumber={pagination.pageNumber}
            pageSize={pagination.pageSize}
            totalCount={totalCount}
            onPageChange={(page: number) => setPagination({ ...pagination, pageNumber: page })}
            ariaLabel="Journal Entry Pagination"
          />

          {/* Journal Entry Modal */}
          <JournalEntryModal
            show={showJournalEntryModal}
            onHide={() => setShowJournalEntryModal(false)}
            onSave={handleSaveJournalEntry}
            journalEntry={selectedJournalEntry}
            accounts={accounts}
          />

          {/* Delete Confirmation Modal */}
          <GenericDeleteConfirmationModal
            show={showDeleteModal}
            onHide={() => setShowDeleteModal(false)}
            onConfirm={handleConfirmDelete}
            loading={deleteLoading}
            itemName={journalEntryToDelete?.entryNumber || ''}
            itemType="journal entry"
            warningMessage="This will permanently delete the journal entry and all its lines."
          />

          {/* Post Journal Entry Modal */}
          <PostJournalEntryModal
            show={showPostModal}
            onHide={() => setShowPostModal(false)}
            onConfirm={handleConfirmPost}
            loading={postLoading}
            journalEntry={journalEntryToPost || null}
          />
        </div>
      </div>
    </div>
  );
};

export default JournalEntriesPage;
