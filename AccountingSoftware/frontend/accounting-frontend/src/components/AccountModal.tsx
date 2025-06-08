import React, { useState, useEffect } from 'react';
import { Account, CreateAccountDto, UpdateAccountDto, AccountType } from '../types';

interface AccountModalProps {
  show: boolean;
  onHide: () => void;
  onSave: (account: CreateAccountDto | UpdateAccountDto) => Promise<void>;
  account?: Account;
  accounts: Account[];
}

const AccountModal: React.FC<AccountModalProps> = ({ show, onHide, onSave, account, accounts }) => {
  const [formData, setFormData] = useState({
    accountCode: '',
    accountName: '',
    accountType: AccountType.Asset,
    description: '',
    parentAccountId: undefined as number | undefined,
    isActive: true
  });
  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState<{[key: string]: string}>({});

  const isEditMode = !!account;

  useEffect(() => {
    if (account) {
      setFormData({
        accountCode: account.accountCode,
        accountName: account.accountName,
        accountType: account.accountType,
        description: account.description,
        parentAccountId: account.parentAccountId,
        isActive: account.isActive
      });
    } else {
      setFormData({
        accountCode: '',
        accountName: '',
        accountType: AccountType.Asset,
        description: '',
        parentAccountId: undefined,
        isActive: true
      });
    }
    setErrors({});
  }, [account, show]);

  const validateForm = (): boolean => {
    const newErrors: {[key: string]: string} = {};

    if (!formData.accountCode.trim()) {
      newErrors.accountCode = 'Account code is required';
    } else if (formData.accountCode.length < 3) {
      newErrors.accountCode = 'Account code must be at least 3 characters';
    }

    if (!formData.accountName.trim()) {
      newErrors.accountName = 'Account name is required';
    }

    if (!formData.description.trim()) {
      newErrors.description = 'Description is required';
    }

    // Check for duplicate account code (only in create mode or if changed in edit mode)
    if (!isEditMode || (isEditMode && formData.accountCode !== account?.accountCode)) {
      const existingAccount = accounts.find(acc => acc.accountCode === formData.accountCode);
      if (existingAccount) {
        newErrors.accountCode = 'Account code already exists';
      }
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }

    setLoading(true);
    try {
      if (isEditMode) {
        const updateData: UpdateAccountDto = {
          accountName: formData.accountName,
          description: formData.description,
          isActive: formData.isActive
        };
        await onSave(updateData);
      } else {
        const createData: CreateAccountDto = {
          accountCode: formData.accountCode,
          accountName: formData.accountName,
          accountType: formData.accountType,
          description: formData.description,
          parentAccountId: formData.parentAccountId
        };
        await onSave(createData);
      }
      onHide();
    } catch (error) {
      console.error('Error saving account:', error);
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

  const getParentAccountOptions = () => {
    return accounts
      .filter(acc => acc.accountType === formData.accountType && acc.id !== account?.id)
      .map(acc => ({ value: acc.id, label: `${acc.accountCode} - ${acc.accountName}` }));
  };

  if (!show) return null;

  return (
    <div className="modal show d-block" tabIndex={-1} style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
      <div className="modal-dialog modal-lg">
        <div className="modal-content">
          <div className="modal-header">
            <h5 className="modal-title">
              {isEditMode ? 'Edit Account' : 'Add New Account'}
            </h5>
            <button type="button" className="btn-close" onClick={onHide}></button>
          </div>
          <form onSubmit={handleSubmit}>
            <div className="modal-body">
              <div className="row">
                <div className="col-md-6">
                  <div className="mb-3">
                    <label htmlFor="accountCode" className="form-label">Account Code *</label>
                    <input
                      type="text"
                      className={`form-control ${errors.accountCode ? 'is-invalid' : ''}`}
                      id="accountCode"
                      value={formData.accountCode}
                      onChange={(e) => setFormData({ ...formData, accountCode: e.target.value })}
                      disabled={isEditMode}
                      placeholder="e.g., 1000"
                    />
                    {errors.accountCode && <div className="invalid-feedback">{errors.accountCode}</div>}
                  </div>
                </div>
                <div className="col-md-6">
                  <div className="mb-3">
                    <label htmlFor="accountName" className="form-label">Account Name *</label>
                    <input
                      type="text"
                      className={`form-control ${errors.accountName ? 'is-invalid' : ''}`}
                      id="accountName"
                      value={formData.accountName}
                      onChange={(e) => setFormData({ ...formData, accountName: e.target.value })}
                      placeholder="e.g., Cash in Bank"
                    />
                    {errors.accountName && <div className="invalid-feedback">{errors.accountName}</div>}
                  </div>
                </div>
              </div>

              <div className="row">
                <div className="col-md-6">
                  <div className="mb-3">
                    <label htmlFor="accountType" className="form-label">Account Type *</label>
                    <select
                      className="form-select"
                      id="accountType"
                      value={formData.accountType}
                      onChange={(e) => setFormData({ ...formData, accountType: parseInt(e.target.value) as AccountType, parentAccountId: undefined })}
                      disabled={isEditMode}
                    >
                      {Object.values(AccountType).filter(value => typeof value === 'number').map((type) => (
                        <option key={type} value={type}>
                          {getAccountTypeName(type as AccountType)}
                        </option>
                      ))}
                    </select>
                  </div>
                </div>
                <div className="col-md-6">
                  <div className="mb-3">
                    <label htmlFor="parentAccount" className="form-label">Parent Account</label>
                    <select
                      className="form-select"
                      id="parentAccount"
                      value={formData.parentAccountId || ''}
                      onChange={(e) => setFormData({ ...formData, parentAccountId: e.target.value ? parseInt(e.target.value) : undefined })}
                      disabled={isEditMode}
                    >
                      <option value="">None (Top Level)</option>
                      {getParentAccountOptions().map(option => (
                        <option key={option.value} value={option.value}>
                          {option.label}
                        </option>
                      ))}
                    </select>
                  </div>
                </div>
              </div>

              <div className="mb-3">
                <label htmlFor="description" className="form-label">Description *</label>
                <textarea
                  className={`form-control ${errors.description ? 'is-invalid' : ''}`}
                  id="description"
                  rows={3}
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  placeholder="Describe the purpose of this account"
                />
                {errors.description && <div className="invalid-feedback">{errors.description}</div>}
              </div>

              {isEditMode && (
                <div className="mb-3">
                  <div className="form-check">
                    <input
                      className="form-check-input"
                      type="checkbox"
                      id="isActive"
                      checked={formData.isActive}
                      onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                    />
                    <label className="form-check-label" htmlFor="isActive">
                      Account is active
                    </label>
                  </div>
                </div>
              )}
            </div>
            <div className="modal-footer">
              <button type="button" className="btn btn-secondary" onClick={onHide}>
                Cancel
              </button>
              <button type="submit" className="btn btn-primary" disabled={loading}>
                {loading ? (
                  <>
                    <span className="spinner-border spinner-border-sm me-2" role="status"></span>
                    {isEditMode ? 'Updating...' : 'Creating...'}
                  </>
                ) : (
                  isEditMode ? 'Update Account' : 'Create Account'
                )}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};

export default AccountModal;