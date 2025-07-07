
import React, { useEffect } from 'react';
import { Account, CreateAccountDto, UpdateAccountDto, AccountType } from '../../types';
import BaseModal from './BaseModal';
import { useFormState } from '../../hooks/useFormState';
import { useValidation } from '../../hooks/useValidation';
import './AccountModal.scss';

interface AccountModalProps {
  show: boolean;
  onHide: () => void;
  onSave: (account: CreateAccountDto | UpdateAccountDto) => Promise<void>;
  account?: Account;
  accounts: Account[];
}

const AccountModal: React.FC<AccountModalProps> = ({ show, onHide, onSave, account, accounts }) => {

  const initialForm = {
    accountCode: '',
    accountName: '',
    accountType: AccountType.Asset,
    description: '',
    parentAccountId: undefined as number | undefined,
    isActive: true
  };
  const { formData, setFormData, handleChange, resetForm } = useFormState(initialForm);
  const [loading, setLoading] = React.useState(false);
  const { errors, setErrors, validate, clearErrors } = useValidation<typeof initialForm>();

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
      resetForm();
    }
    clearErrors();
  }, [account, show, setFormData, resetForm, clearErrors]);

  const validateForm = () =>
    validate(() => {
      const newErrors: { [key: string]: string } = {};
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
      if (!isEditMode || (isEditMode && formData.accountCode !== account?.accountCode)) {
        const existingAccount = accounts.find(acc => acc.accountCode === formData.accountCode);
        if (existingAccount) {
          newErrors.accountCode = 'Account code already exists';
        }
      }
      return newErrors;
    });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validateForm()) return;
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

  const modalTitle = (
    <>
      <i className="bi bi-folder-plus me-2"></i>
      {isEditMode ? 'Edit Account' : 'Add New Account'}
    </>
  );

  const modalFooter = (
    <>
      <button type="button" className="btn btn-secondary" onClick={onHide}>
        <i className="bi bi-x-circle me-2"></i>
        Cancel
      </button>
      <button type="submit" className="btn btn-primary save-btn" disabled={loading}>
        {loading ? (
          <>
            <span className="spinner-border spinner-border-sm me-2" role="status"></span>
            {isEditMode ? 'Updating...' : 'Creating...'}
          </>
        ) : (
          <>
            <i className="bi bi-check-circle me-2"></i>
            {isEditMode ? 'Update Account' : 'Create Account'}
          </>
        )}
      </button>
    </>
  );

  return (
    <BaseModal
      show={show}
      onHide={onHide}
      title={modalTitle}
      footer={null}
      size="lg"
      className="account-modal"
      ariaLabel={isEditMode ? 'Edit Account Modal' : 'Add Account Modal'}
    >
      <form onSubmit={handleSubmit}>
        <div className="row">
          <div className="col-md-6">
            <div className="mb-3">
              <label htmlFor="accountCode" className="form-label">Account Code *</label>
          <input
            type="text"
            className={`form-control ${errors.accountCode ? 'is-invalid' : ''}`}
            id="accountCode"
            name="accountCode"
            value={formData.accountCode}
            onChange={handleChange}
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
            name="accountName"
            value={formData.accountName}
            onChange={handleChange}
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
                name="accountType"
                value={formData.accountType}
                onChange={(e) => {
                  handleChange(e);
                  setFormData(prev => ({ ...prev, parentAccountId: undefined }));
                }}
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
                name="parentAccountId"
                value={formData.parentAccountId || ''}
                onChange={(e) => {
                  const value = e.target.value ? parseInt(e.target.value) : undefined;
                  setFormData(prev => ({ ...prev, parentAccountId: value }));
                }}
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
            name="description"
            rows={3}
            value={formData.description}
            onChange={handleChange}
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
            name="isActive"
            checked={formData.isActive}
            onChange={handleChange}
          />
              <label className="form-check-label" htmlFor="isActive">
                Account is active
              </label>
            </div>
          </div>
        )}

        <div className="modal-footer">
          {modalFooter}
        </div>
      </form>
    </BaseModal>
  );
};

export default AccountModal;