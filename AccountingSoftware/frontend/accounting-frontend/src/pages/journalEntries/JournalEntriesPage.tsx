import React, { useState, useEffect } from 'react';
import { JournalEntry, CreateJournalEntryDto, UpdateJournalEntryDto, Account } from '../../types';
import { journalEntryService, accountService } from '../../services/api';
import { usePageTitle } from '../../hooks/usePageTitle';
import JournalEntryModal from '../../components/modals/JournalEntryModal';
import GenericDeleteConfirmationModal from '../../components/modals/GenericDeleteConfirmationModal';

const JournalEntriesPage: React.FC = () => {
  usePageTitle('Journal Entries');

  const [journalEntries, setJournalEntries] = useState<JournalEntry[]>([]);
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [expandedRows, setExpandedRows] = useState<Set<number>>(new Set());
  const [searchTerm, setSearchTerm] = useState('');
  const [filterBy, setFilterBy] = useState<'all' | 'posted' | 'unposted'>('all');
  
  // Modal states
  const [showJournalEntryModal, setShowJournalEntryModal] = useState(false);
  const [selectedJournalEntry, setSelectedJournalEntry] = useState<JournalEntry | undefined>();
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [journalEntryToDelete, setJournalEntryToDelete] = useState<JournalEntry | undefined>();
  const [deleteLoading, setDeleteLoading] = useState(false);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      const [journalEntriesData, accountsData] = await Promise.all([
        journalEntryService.getJournalEntries(),
        accountService.getAccounts()
      ]);
      setJournalEntries(journalEntriesData);
      setAccounts(accountsData);
      setError(null);
    } catch (err) {
      setError('Failed to load data');
      console.error('Error loading data:', err);
    } finally {
      setLoading(false);
    }
  };

  const toggleRowExpansion = (id: number) => {
    const newExpandedRows = new Set(expandedRows);
    if (newExpandedRows.has(id)) {
      newExpandedRows.delete(id);
    } else {
      newExpandedRows.add(id);
    }
    setExpandedRows(newExpandedRows);
  };

  const filteredJournalEntries = journalEntries.filter(entry => {
    const matchesSearch = 
      entry.entryNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
      entry.description.toLowerCase().includes(searchTerm.toLowerCase()) ||
      entry.reference.toLowerCase().includes(searchTerm.toLowerCase());

    const matchesFilter = 
      filterBy === 'all' ||
      (filterBy === 'posted' && entry.isPosted) ||
      (filterBy === 'unposted' && !entry.isPosted);

    return matchesSearch && matchesFilter;
  });

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
        // Update existing entry
        await journalEntryService.updateJournalEntry(entryId, journalEntryData as UpdateJournalEntryDto);
      } else {
        // Create new entry
        await journalEntryService.createJournalEntry(journalEntryData as CreateJournalEntryDto);
      }
      await loadData(); // Reload data
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
      await loadData(); // Reload data
      setShowDeleteModal(false);
      setJournalEntryToDelete(undefined);
    } catch (error) {
      console.error('Error deleting journal entry:', error);
    } finally {
      setDeleteLoading(false);
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
      <button className="btn btn-sm btn-outline-danger ms-2" onClick={loadData}>
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
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                />
              </div>
            </div>
            <div className="col-md-3">
              <select
                className="form-select"
                value={filterBy}
                onChange={(e) => setFilterBy(e.target.value as 'all' | 'posted' | 'unposted')}
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
                  <thead className="table-dark">
                    <tr>
                      <th scope="col" style={{width: '50px'}}></th>
                      <th scope="col">Entry #</th>
                      <th scope="col">Date</th>
                      <th scope="col">Description</th>
                      <th scope="col">Reference</th>
                      <th scope="col">Total</th>
                      <th scope="col">Status</th>
                      <th scope="col">Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {filteredJournalEntries.map((entry) => (
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
                                        {entry.lines.map((line) => (
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
                                            {formatCurrency(entry.lines.reduce((sum, line) => sum + line.debitAmount, 0))}
                                          </td>
                                          <td className="text-end fw-bold">
                                            {formatCurrency(entry.lines.reduce((sum, line) => sum + line.creditAmount, 0))}
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

          {filteredJournalEntries.length === 0 && !loading && (
            <div className="text-center py-5">
              <div className="mb-3">
                <i className="bi bi-journal-text display-1 text-muted"></i>
              </div>
              <h5 className="text-muted">
                {searchTerm || filterBy !== 'all' ? 'No entries match your criteria' : 'No journal entries found'}
              </h5>
              <p className="text-muted">
                {searchTerm || filterBy !== 'all' 
                  ? 'Try adjusting your search or filter settings.' 
                  : 'Journal entries are automatically created when invoices are generated or paid.'}
              </p>
              {(!searchTerm && filterBy === 'all') && (
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
          {filteredJournalEntries.length > 0 && (
            <div className="mt-3 text-muted text-center">
              Showing {filteredJournalEntries.length} of {journalEntries.length} journal entries
            </div>
          )}

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
        </div>
      </div>
    </div>
  );
};

export default JournalEntriesPage;
