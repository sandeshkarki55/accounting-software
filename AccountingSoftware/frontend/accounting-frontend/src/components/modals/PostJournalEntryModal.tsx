import React from 'react';
import { JournalEntry } from '../../types';

interface PostJournalEntryModalProps {
  show: boolean;
  onHide: () => void;
  onConfirm: () => Promise<void>;
  journalEntry: JournalEntry | null;
  loading: boolean;
}

const PostJournalEntryModal: React.FC<PostJournalEntryModalProps> = ({
  show,
  onHide,
  onConfirm,
  journalEntry,
  loading
}) => {
  if (!show || !journalEntry) return null;

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-US', { 
      style: 'currency', 
      currency: 'USD' 
    }).format(amount);
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  };

  const handleConfirm = async () => {
    await onConfirm();
    onHide();
  };

  return (
    <div className="modal show d-block" tabIndex={-1} style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
      <div className="modal-dialog">
        <div className="modal-content">
          <div className="modal-header">
            <h5 className="modal-title">
              <i className="bi bi-check-circle me-2 text-success"></i>
              Post Journal Entry
            </h5>
            <button type="button" className="btn-close" onClick={onHide}></button>
          </div>
          
          <div className="modal-body">
            <div className="alert alert-warning mb-3">
              <i className="bi bi-exclamation-triangle me-2"></i>
              <strong>Important:</strong> Once posted, this journal entry cannot be modified or deleted.
            </div>

            <div className="mb-3">
              <h6>Journal Entry Details:</h6>
              <div className="card">
                <div className="card-body">
                  <div className="row">
                    <div className="col-sm-4">
                      <strong>Entry Number:</strong>
                    </div>
                    <div className="col-sm-8">
                      {journalEntry.entryNumber}
                    </div>
                  </div>
                  <div className="row">
                    <div className="col-sm-4">
                      <strong>Transaction Date:</strong>
                    </div>
                    <div className="col-sm-8">
                      {formatDate(journalEntry.transactionDate)}
                    </div>
                  </div>
                  <div className="row">
                    <div className="col-sm-4">
                      <strong>Description:</strong>
                    </div>
                    <div className="col-sm-8">
                      {journalEntry.description}
                    </div>
                  </div>
                  {journalEntry.reference && (
                    <div className="row">
                      <div className="col-sm-4">
                        <strong>Reference:</strong>
                      </div>
                      <div className="col-sm-8">
                        {journalEntry.reference}
                      </div>
                    </div>
                  )}
                  <div className="row">
                    <div className="col-sm-4">
                      <strong>Total Amount:</strong>
                    </div>
                    <div className="col-sm-8">
                      {formatCurrency(journalEntry.totalAmount)}
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <div className="mb-3">
              <h6>Journal Entry Lines:</h6>
              <div className="table-responsive">
                <table className="table table-sm">
                  <thead>
                    <tr>
                      <th>Account</th>
                      <th>Description</th>
                      <th className="text-end">Debit</th>
                      <th className="text-end">Credit</th>
                    </tr>
                  </thead>
                  <tbody>
                    {journalEntry.lines.map((line, index) => (
                      <tr key={index}>
                        <td>
                          <small className="text-muted">{line.accountCode}</small><br/>
                          {line.accountName}
                        </td>
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
                      <td colSpan={2} className="fw-bold">Totals:</td>
                      <td className="text-end fw-bold">
                        {formatCurrency(journalEntry.lines.reduce((sum, line) => sum + line.debitAmount, 0))}
                      </td>
                      <td className="text-end fw-bold">
                        {formatCurrency(journalEntry.lines.reduce((sum, line) => sum + line.creditAmount, 0))}
                      </td>
                    </tr>
                  </tfoot>
                </table>
              </div>
            </div>

            <div className="alert alert-info">
              <i className="bi bi-info-circle me-2"></i>
              Posting this entry will update the account balances and make the entry permanent.
            </div>
          </div>
          
          <div className="modal-footer">
            <button type="button" className="btn btn-secondary" onClick={onHide}>
              Cancel
            </button>
            <button 
              type="button" 
              className="btn btn-success" 
              onClick={handleConfirm}
              disabled={loading}
            >
              {loading ? (
                <>
                  <span className="spinner-border spinner-border-sm me-2" role="status"></span>
                  Posting Entry...
                </>
              ) : (
                <>
                  <i className="bi bi-check-circle me-2"></i>
                  Post Entry
                </>
              )}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default PostJournalEntryModal;
