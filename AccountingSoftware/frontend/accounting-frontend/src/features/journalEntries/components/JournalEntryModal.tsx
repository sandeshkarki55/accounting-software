import React, { useEffect } from 'react';
import { JournalEntry, CreateJournalEntryDto, UpdateJournalEntryDto, CreateJournalEntryLineDto, Account } from '../../../types';
import BaseModal from '../../../components/shared/BaseModal';
import { useFormState } from '../../../hooks/useFormState';
import { useValidation } from '../../../hooks/useValidation';

interface JournalEntryModalProps {
  show: boolean;
  onHide: () => void;
  onSave: (journalEntry: CreateJournalEntryDto | UpdateJournalEntryDto, isUpdate?: boolean, entryId?: number) => Promise<void>;
  journalEntry?: JournalEntry;
  accounts: Account[];
}

const JournalEntryModal: React.FC<JournalEntryModalProps> = ({ show, onHide, onSave, journalEntry, accounts }) => {
  // Support both paged and non-paged data
  const accountList: Account[] = Array.isArray(accounts)
    ? accounts
    : (accounts && Array.isArray((accounts as any).items))
      ? (accounts as any).items
      : [];

  const initialForm = {
    transactionDate: new Date().toISOString().split('T')[0],
    description: '',
    reference: '',
  };
  const { formData, setFormData, handleChange, resetForm } = useFormState(initialForm);
  const { errors, setErrors, validate, clearErrors } = useValidation<Record<string, string>>();
  const [lines, setLines] = React.useState<CreateJournalEntryLineDto[]>([
    { accountId: 0, debitAmount: 0, creditAmount: 0, description: '' },
    { accountId: 0, debitAmount: 0, creditAmount: 0, description: '' }
  ]);
  const [loading, setLoading] = React.useState(false);
  const isViewMode = !!journalEntry && journalEntry.isPosted;
  const isEditMode = !!journalEntry && !journalEntry.isPosted;

  useEffect(() => {
    if (journalEntry) {
      setFormData({
        transactionDate: journalEntry.transactionDate.split('T')[0],
        description: journalEntry.description,
        reference: journalEntry.reference,
      });
      setLines(journalEntry.lines.map(line => ({
        accountId: line.accountId,
        debitAmount: line.debitAmount,
        creditAmount: line.creditAmount,
        description: line.description
      })));
    } else {
      resetForm();
      setLines([
        { accountId: 0, debitAmount: 0, creditAmount: 0, description: '' },
        { accountId: 0, debitAmount: 0, creditAmount: 0, description: '' }
      ]);
    }
    clearErrors();
  }, [journalEntry, show, setFormData, resetForm, clearErrors]);

  const validateForm = () =>
    validate(() => {
      const newErrors: { [key: string]: string } = {};
      if (!formData.transactionDate) {
        newErrors.transactionDate = 'Transaction date is required';
      }
      if (!formData.description.trim()) {
        newErrors.description = 'Description is required';
      }
      // Validate lines
      const validLines = lines.filter(line => line.accountId !== 0);
      if (validLines.length < 2) {
        newErrors.lines = 'At least two account lines are required';
      }
      // Check for balanced debits and credits
      const totalDebits = validLines.reduce((sum, line) => sum + line.debitAmount, 0);
      const totalCredits = validLines.reduce((sum, line) => sum + line.creditAmount, 0);
      if (Math.abs(totalDebits - totalCredits) > 0.01) {
        newErrors.balance = 'Total debits must equal total credits';
      }
      // Validate each line
      validLines.forEach((line, index) => {
        if (line.debitAmount === 0 && line.creditAmount === 0) {
          newErrors[`line_${index}_amount`] = 'Either debit or credit amount is required';
        }
        if (line.debitAmount > 0 && line.creditAmount > 0) {
          newErrors[`line_${index}_amount`] = 'Cannot have both debit and credit amounts';
        }
      });
      return newErrors;
    });

  const handleLineChange = (index: number, field: keyof CreateJournalEntryLineDto, value: string | number) => {
    const newLines = [...lines];
    newLines[index] = { ...newLines[index], [field]: value };
    // If changing debit, clear credit and vice versa
    if (field === 'debitAmount' && parseFloat(value.toString()) > 0) {
      newLines[index].creditAmount = 0;
    } else if (field === 'creditAmount' && parseFloat(value.toString()) > 0) {
      newLines[index].debitAmount = 0;
    }
    setLines(newLines);
    // Clear error for this line if present
    if (errors[`line_${index}_amount`]) {
      setErrors((prev: any) => ({ ...prev, [`line_${index}_amount`]: '' }));
    }
  };

  const addLine = () => {
    setLines([...lines, { accountId: 0, debitAmount: 0, creditAmount: 0, description: '' }]);
  };

  const removeLine = (index: number) => {
    if (lines.length > 2) {
      const newLines = lines.filter((_, i) => i !== index);
      setLines(newLines);
    }
  };

  const calculateTotals = () => {
    const validLines = lines.filter(line => line.accountId !== 0);
    const totalDebits = validLines.reduce((sum, line) => sum + line.debitAmount, 0);
    const totalCredits = validLines.reduce((sum, line) => sum + line.creditAmount, 0);
    return { totalDebits, totalCredits };
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validateForm()) return;
    setLoading(true);
    try {
      const validLines = lines.filter(line => line.accountId !== 0);
      if (isEditMode && journalEntry) {
        const updateData: UpdateJournalEntryDto = {
          ...formData,
          lines: validLines
        };
        await onSave(updateData, true, journalEntry.id);
      } else {
        const createData: CreateJournalEntryDto = {
          ...formData,
          lines: validLines
        };
        await onSave(createData, false);
      }
      handleClose();
    } catch (error) {
      console.error('Error saving journal entry:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    resetForm();
    clearErrors();
    setLines([
      { accountId: 0, debitAmount: 0, creditAmount: 0, description: '' },
      { accountId: 0, debitAmount: 0, creditAmount: 0, description: '' }
    ]);
    onHide();
  };

  const formatCurrency = (amount: number) => new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount);

  const getAccountName = (accountId: number) => {
    const account = accountList.find(acc => acc.id === accountId);
    return account ? `${account.accountCode} - ${account.accountName}` : '';
  };

  const { totalDebits, totalCredits } = calculateTotals();
  const isBalanced = Math.abs(totalDebits - totalCredits) < 0.01;
  if (!show) return null;

  const modalTitle = (
    <>
      <i className="bi bi-journal-text me-2"></i>
      {isViewMode
        ? `Journal Entry ${journalEntry?.entryNumber}`
        : isEditMode
        ? `Edit Journal Entry ${journalEntry?.entryNumber}`
        : 'Create New Journal Entry'}
    </>
  );

  const modalFooter = (
    <>
      <button type="button" className="btn btn-secondary" onClick={handleClose} disabled={loading}>
        <i className="bi bi-x-circle me-2"></i>
        {isViewMode ? 'Close' : 'Cancel'}
      </button>
      {!isViewMode && (
        <button type="submit" className="btn btn-primary save-btn" disabled={loading || !isBalanced}>
          {loading ? (
            <>
              <span className="spinner-border spinner-border-sm me-2" role="status"></span>
              {isEditMode ? 'Updating Entry...' : 'Creating Entry...'}
            </>
          ) : (
            <>
              <i className="bi bi-journal-text me-2"></i>
              {isEditMode ? 'Update Entry' : 'Create Entry'}
            </>
          )}
        </button>
      )}
    </>
  );

  return (
    <BaseModal
      show={show}
      onHide={handleClose}
      title={modalTitle}
      footer={null}
      size="xl"
      className="journal-entry-modal"
      ariaLabel={isEditMode ? 'Edit Journal Entry Modal' : 'Add Journal Entry Modal'}
    >
      <form onSubmit={handleSubmit}>
        <div className="modal-body">
          {isViewMode && journalEntry && (
            <div className="alert alert-info mb-3">
              <i className="bi bi-info-circle me-2"></i>
              Entry Number: <strong>{journalEntry.entryNumber}</strong>
              {journalEntry.isPosted && (
                <span className="badge bg-success ms-2">Posted</span>
              )}
              {!journalEntry.isPosted && (
                <span className="badge bg-warning text-dark ms-2">Draft</span>
              )}
              <div className="mt-2">
                <small className="text-muted">
                  <i className="bi bi-info-circle me-1"></i>
                  Posted journal entries cannot be modified.
                </small>
              </div>
            </div>
          )}
          {isEditMode && journalEntry && (
            <div className="alert alert-warning mb-3">
              <i className="bi bi-pencil me-2"></i>
              Editing Entry: <strong>{journalEntry.entryNumber}</strong>
              <span className="badge bg-warning text-dark ms-2">Draft</span>
              <div className="mt-2">
                <small className="text-muted">
                  <i className="bi bi-info-circle me-1"></i>
                  Only unposted journal entries can be edited.
                </small>
              </div>
            </div>
          )}
          <div className="row">
            <div className="col-md-4">
              <div className="mb-3">
                <label htmlFor="transactionDate" className="form-label">Transaction Date *</label>
                <input
                  type="date"
                  className={`form-control ${errors.transactionDate ? 'is-invalid' : ''}`}
                  id="transactionDate"
                  name="transactionDate"
                  value={formData.transactionDate}
                  onChange={handleChange}
                  disabled={isViewMode}
                />
                {errors.transactionDate && <div className="invalid-feedback">{errors.transactionDate}</div>}
              </div>
            </div>
            <div className="col-md-4">
              <div className="mb-3">
                <label htmlFor="reference" className="form-label">Reference</label>
                <input
                  type="text"
                  className="form-control"
                  id="reference"
                  name="reference"
                  value={formData.reference}
                  onChange={handleChange}
                  placeholder="Reference number or code"
                  disabled={isViewMode}
                />
              </div>
            </div>
            <div className="col-md-4">
              <div className="mb-3">
                <label className="form-label">Balance Status</label>
                <div className={`alert ${isBalanced ? 'alert-success' : 'alert-warning'} py-2 mb-0`}>
                  <i className={`bi ${isBalanced ? 'bi-check-circle' : 'bi-exclamation-triangle'} me-2`}></i>
                  {isBalanced ? 'Balanced' : 'Out of Balance'}
                </div>
              </div>
            </div>
          </div>
          <div className="mb-3">
            <label htmlFor="description" className="form-label">Description *</label>
            <input
              type="text"
              className={`form-control ${errors.description ? 'is-invalid' : ''}`}
              id="description"
              name="description"
              value={formData.description}
              onChange={handleChange}
              placeholder="Brief description of the journal entry"
              disabled={isViewMode}
            />
            {errors.description && <div className="invalid-feedback">{errors.description}</div>}
          </div>
          {/* Journal Entry Lines */}
          <div className="mb-4">
            <div className="d-flex justify-content-between align-items-center mb-3">
              <h6 className="mb-0">Journal Entry Lines</h6>
              {!isViewMode && (
                <button type="button" className="btn btn-sm btn-outline-primary" onClick={addLine}>
                  <i className="bi bi-plus-circle me-1"></i>
                  Add Line
                </button>
              )}
            </div>
            {errors.lines && <div className="alert alert-danger">{errors.lines}</div>}
            {errors.balance && <div className="alert alert-danger">{errors.balance}</div>}
            <div className="table-responsive">
              <table className="table table-sm">
                <thead>
                  <tr>
                    <th>Account</th>
                    <th>Description</th>
                    <th style={{width: '120px'}}>Debit</th>
                    <th style={{width: '120px'}}>Credit</th>
                    {!isViewMode && <th style={{width: '50px'}}>Actions</th>}
                  </tr>
                </thead>
                <tbody>
                  {lines.map((line, index) => (
                    <tr key={index}>
                      <td>
                        {isViewMode ? (
                          <span>{getAccountName(line.accountId)}</span>
                        ) : (
                          <select
                            className="form-select form-select-sm"
                            value={line.accountId}
                            onChange={e => handleLineChange(index, 'accountId', parseInt(e.target.value))}
                          >
                            <option value={0}>Select account...</option>
                            {accountList.filter(acc => acc.isActive).map((account) => (
                              <option key={account.id} value={account.id}>
                                {account.accountCode} - {account.accountName}
                              </option>
                            ))}
                          </select>
                        )}
                      </td>
                      <td>
                        <input
                          type="text"
                          className="form-control form-control-sm"
                          value={line.description}
                          onChange={e => handleLineChange(index, 'description', e.target.value)}
                          placeholder="Line description"
                          disabled={isViewMode}
                        />
                      </td>
                      <td>
                        <input
                          type="number"
                          className="form-control form-control-sm"
                          value={line.debitAmount || ''}
                          onChange={e => handleLineChange(index, 'debitAmount', parseFloat(e.target.value) || 0)}
                          min="0"
                          step="0.01"
                          disabled={isViewMode}
                        />
                        {errors[`line_${index}_amount`] && (
                          <small className="text-danger">{errors[`line_${index}_amount`]}</small>
                        )}
                      </td>
                      <td>
                        <input
                          type="number"
                          className="form-control form-control-sm"
                          value={line.creditAmount || ''}
                          onChange={e => handleLineChange(index, 'creditAmount', parseFloat(e.target.value) || 0)}
                          min="0"
                          step="0.01"
                          disabled={isViewMode}
                        />
                      </td>
                      {!isViewMode && (
                        <td>
                          <button
                            type="button"
                            className="btn btn-sm btn-outline-danger"
                            onClick={() => removeLine(index)}
                            disabled={lines.length === 2}
                          >
                            <i className="bi bi-trash"></i>
                          </button>
                        </td>
                      )}
                    </tr>
                  ))}
                </tbody>
                <tfoot>
                  <tr className="table-secondary">
                    <td colSpan={2} className="fw-bold">Totals:</td>
                    <td className="fw-bold">{formatCurrency(totalDebits)}</td>
                    <td className="fw-bold">{formatCurrency(totalCredits)}</td>
                    {!isViewMode && <td></td>}
                  </tr>
                </tfoot>
              </table>
            </div>
          </div>
        </div>
        <div className="modal-footer">{modalFooter}</div>
      </form>
    </BaseModal>
  );
};

export default JournalEntryModal;
